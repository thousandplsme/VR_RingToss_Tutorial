"""Regression checks for missed defects and unsafe automation paths; no Unity required."""
import contextlib
import io
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

import verify_project as verify


def temporary_directory():
    parent = verify.ROOT / "Docs/CourseValidation/Efficiency/runs/unit-tests"
    if not parent.resolve().is_relative_to(verify.ROOT.resolve()):
        raise RuntimeError("Test temporary directory must stay inside the workspace")
    parent.mkdir(parents=True, exist_ok=True)
    return tempfile.TemporaryDirectory(dir=parent)


class ChapterChecks(unittest.TestCase):
    def test_missing_target_in_chapter_three_is_rejected(self):
        self.assertFalse(verify.inspect_chapter("", 3, "score", "controls")["target"])

    def test_missing_score_in_chapter_four_is_rejected(self):
        self.assertFalse(verify.inspect_chapter("  m_Name: Target\n", 4, "score", "controls")["score"])

    def test_later_chapter_components_are_rejected(self):
        result = verify.inspect_chapter("  m_Name: Target\n guid: score\n guid: controls\n", 3, "score", "controls")
        self.assertFalse(result["score"])
        self.assertFalse(result["controls"])

    def test_final_chapter_presence(self):
        self.assertTrue(all(verify.inspect_chapter("  m_Name: Target\n guid: score\n guid: controls\n", 5, "score", "controls").values()))


class SafeExecution(unittest.TestCase):
    def test_invalid_timeout_is_rejected_before_execution(self):
        with patch("sys.argv", ["verify_project.py", "check", "--timeout", "0"]), \
             patch.object(verify, "run") as run, contextlib.redirect_stderr(io.StringIO()):
            with self.assertRaises(SystemExit):
                verify.main()
            run.assert_not_called()

    def test_suite_cannot_be_reported_for_a_non_play_task(self):
        with patch("sys.argv", ["verify_project.py", "check", "--task", "audit", "--suite", "input"]), \
             patch.object(verify, "run") as run, contextlib.redirect_stderr(io.StringIO()):
            with self.assertRaises(SystemExit):
                verify.main()
            run.assert_not_called()

    def test_only_exact_known_urp_normalization_can_be_restored(self):
        before = b"  k_AssetVersion: 13\n  k_AssetPreviousVersion: 12\n  m_ShadowDistance: 50\n"
        after = before.replace(b"PreviousVersion: 12", b"PreviousVersion: 13")
        self.assertTrue(verify.is_known_urp_normalization(before, after))
        self.assertFalse(verify.is_known_urp_normalization(before, after.replace(b"Distance: 50", b"Distance: 10")))
        self.assertFalse(verify.is_known_urp_normalization(before, before))

    def test_unity_cli_is_not_mistaken_for_an_editor(self):
        self.assertFalse(verify.is_editor_process(r"C:\Users\me\AppData\Local\Unity\bin\unity.exe", "C:/Editor/Unity.exe"))
        self.assertFalse(verify.is_editor_process(r"C:\Program Files\Unity Hub\resources\unity.exe", "C:/Editor/Unity.exe"))
        self.assertTrue(verify.is_editor_process(r"C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe", "C:/Editor/Unity.exe"))
        self.assertTrue(verify.is_editor_process("D:/Custom/Unity.exe", "D:/Custom/Unity.exe"))

    def test_asynchronous_play_command_does_not_force_quit_or_remove_graphics(self):
        command = verify.editor_command(Path("D:/Editor/Unity.exe"), Path("D:/reports/a b"), "play", "input")
        self.assertNotIn("-quit", command)
        self.assertNotIn("-nographics", command)
        self.assertEqual(command[command.index("-projectPath") + 1], str(verify.PROJECT))
        self.assertEqual(command[command.index("-courseSuite") + 1], "input")

    def test_snapshot_refuses_overwrite(self):
        with temporary_directory() as folder, patch.object(verify, "EVIDENCE", Path(folder)), \
             patch.object(verify, "inputs", return_value={"Assets/a": "HASH"}), \
             patch("sys.argv", ["verify_project.py", "snapshot"]), \
             contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
            self.assertEqual(verify.main(), 0)
            path = Path(folder) / "before.json"
            before = path.read_bytes()
            with self.assertRaises(SystemExit):
                verify.main()
            self.assertEqual(before, path.read_bytes())

    def test_runtime_and_settings_are_protected_but_developer_code_can_change(self):
        self.assertTrue(verify.protected("Assets/RingTossTutorial/RingInitialPlacement.cs"))
        self.assertTrue(verify.protected("CourseValidation/VRRT/ProjectSettings/ProjectSettings.asset"))
        self.assertTrue(verify.protected("CourseValidation/VRRT/Assets/Scripts/RingScore.cs"))
        self.assertFalse(verify.protected("CourseValidation/VRRT/Assets/Editor/CourseAudit.cs"))


class EvidenceChecks(unittest.TestCase):
    def verdict(self, value):
        with temporary_directory() as folder:
            verify.write_json(Path(folder) / "play-tests.json", value)
            return verify.report_result("play", Path(folder))

    def test_success_flag_does_not_hide_a_failed_test(self):
        self.assertFalse(self.verdict({"success": True, "tests": [{"passed": False}]}))

    def test_empty_or_entirely_skipped_tests_are_not_a_pass(self):
        self.assertFalse(self.verdict({"success": True, "tests": []}))
        self.assertFalse(self.verdict({"success": True, "tests": [{"passed": None, "skipped": True}]}))

    def test_failure_flag_wins_over_passed_assertions(self):
        self.assertFalse(self.verdict({"success": False, "tests": [{"passed": True}]}))

    def test_partial_skip_is_explicit_and_other_tests_must_pass(self):
        self.assertTrue(self.verdict({"success": True, "tests": [{"passed": True}, {"passed": None, "skipped": True}]}))

    def test_build_requires_the_recorded_artifact_hash(self):
        with temporary_directory() as folder:
            path = Path(folder)
            apk = path / "fake.apk"
            apk.write_bytes(b"test artifact")
            verify.write_json(path / "android-build.json", {"result": "Succeeded", "totalErrors": 0, "output": str(apk)})
            verify.write_json(path / "apk-artifact.json", {"sha256": "WRONG"})
            self.assertFalse(verify.report_result("build", path))
            verify.write_json(path / "apk-artifact.json", {"sha256": verify.digest(apk)})
            self.assertTrue(verify.report_result("build", path))


if __name__ == "__main__":
    unittest.main()
