"""Developer checks for the two ring-toss projects (Python standard library only)."""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tarfile
import time
import uuid

ROOT = Path(__file__).resolve().parents[2]
PROJECT = ROOT / "CourseValidation/VRRT"
EVIDENCE = ROOT / "Docs/CourseValidation/Efficiency"
SCRIPTS = ("PlaceRingAtStart", "RingPracticeControls", "RingScore", "RingScoreDisplay")
SCENES = ("01_Grab_Complete", "02_Throw", "03_Target", "04_Score", "05_RingToss")


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest().upper()


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def write_json(path, data):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def inputs():
    paths = []
    for project in (ROOT, PROJECT):
        for area in ("Assets", "Packages", "ProjectSettings"):
            paths.extend(p for p in (project / area).rglob("*") if p.is_file())
    paths.extend((ROOT / "Docs/tools").glob("*.py"))
    paths.extend((ROOT / "Docs/CourseValidation/Sources").glob("*.cs"))
    return {p.relative_to(ROOT).as_posix(): digest(p) for p in sorted(set(paths))}


def protected(path):
    # All original project files, and validation assets/settings except developer Editor code.
    return (path.startswith(("Assets/", "Packages/", "ProjectSettings/")) or
            path.startswith("CourseValidation/VRRT/") and
            not path.startswith("CourseValidation/VRRT/Assets/Editor/") and
            path != "CourseValidation/VRRT/Assets/Editor.meta")


def normalized(text):
    return text.replace("\r\n", "\n").strip()


def inspect_chapter(text, chapter, score_guid, controls_guid):
    """A static presence check; Unity's CourseAudit checks the actual object references."""
    target = bool(re.search(r"^  m_Name: Target\s*$", text, re.M))
    score = f"guid: {score_guid}" in text
    controls = f"guid: {controls_guid}" in text
    return {"target": target == (chapter >= 3), "score": score == (chapter >= 4),
            "controls": controls == (chapter == 5)}


def static_checks(protect_baseline=False):
    results = []

    def check(name, passed, detail=None):
        results.append({"name": name, "passed": bool(passed), "detail": detail})

    for name in SCRIPTS:
        source = (ROOT / f"Docs/CourseValidation/Sources/{name}.cs").read_text(encoding="utf-8-sig")
        runtime = (PROJECT / f"Assets/Scripts/{name}.cs").read_text(encoding="utf-8-sig")
        check(f"source_copy:{name}", normalized(source) == normalized(runtime))
        blocks = []
        for page in (ROOT / "Docs/CourseValidation/NotionSnapshot").glob("ch*.md"):
            blocks += [block for block in re.findall(r"```(?:cs|csharp)\s*\n(.*?)```", page.read_text(encoding="utf-8-sig"), re.S)
                       if re.search(r"\bclass\s+" + re.escape(name) + r"\b", block)]
        check(f"notion_snapshot:{name}", len(blocks) == 1 and normalized(blocks[0]) == normalized(runtime),
              "Local snapshot only; live Notion is not checked")
    check("audit_source_copy", normalized((ROOT / "Docs/CourseValidation/CourseAudit.cs").read_text(encoding="utf-8-sig")) ==
          normalized((PROJECT / "Assets/Editor/CourseAudit.cs").read_text(encoding="utf-8-sig")))
    guids = {name: re.search(r"^guid: (\w+)$", (PROJECT / f"Assets/Scripts/{name}.cs.meta").read_text(), re.M)[1]
             for name in ("RingScore", "RingPracticeControls")}
    for chapter, name in enumerate(SCENES, 1):
        text = (PROJECT / f"Assets/Scenes/{name}.unity").read_text(encoding="utf-8-sig")
        for condition, ok in inspect_chapter(text, chapter, guids["RingScore"], guids["RingPracticeControls"]).items():
            check(f"chapter:{name}:{condition}", ok)
    package = ROOT / "Docs/CourseValidation/Distribution/RingShape_v2.unitypackage"
    with tarfile.open(package, "r:gz") as archive:
        assets = {m.name.split("/")[0]: m for m in archive.getmembers() if m.name.endswith("/asset")}
        prefab = next((m for m in assets.values() if (member := archive.getmember(m.name.rsplit("/", 1)[0] + "/pathname")) and
                       archive.extractfile(member).read().decode().endswith("RingShape.prefab")), None)
        refs = set(re.findall(rb"guid: ([0-9a-f]{32})", archive.extractfile(prefab).read())) if prefab else set()
        check("package_dependencies", prefab is not None and bool(refs) and all(g.decode() in assets for g in refs),
              "RingShape prefab mesh/material included; archive is inspected without extraction")
    if protect_baseline:
        baseline = read_json(EVIDENCE / "before.json")
        changed = [path for path, sha in baseline["files"].items() if protected(path) and
                   (not (ROOT / path).is_file() or digest(ROOT / path) != sha)]
        check("protected_files_unchanged", not changed, changed)
    return results


def editor_command(editor, run_dir, task, suite):
    # This custom asynchronous harness must not receive -quit or -nographics.
    return [str(editor), "-batchmode", "-projectPath", str(PROJECT), "-executeMethod", "CourseValidationRunner.Run",
            "-courseTask", task, "-courseSuite", suite, "-courseReportDirectory", str(run_dir),
            "-logFile", str(run_dir / "editor.log")]


def is_editor_process(executable, expected_editor):
    path = str(executable).replace("\\", "/").lower()
    return path.endswith("/editor/unity.exe") or path == str(expected_editor).replace("\\", "/").lower()


def is_known_urp_normalization(before, after):
    marker = b"  k_AssetPreviousVersion: 12"
    return marker in before and b"  k_AssetVersion: 13" in before and before != after and after == before.replace(
        marker, b"  k_AssetPreviousVersion: 13", 1)


def restore_urp_normalization(path, before):
    after = path.read_bytes()
    if is_known_urp_normalization(before, after):
        path.write_bytes(before)
        return [{"path": path.relative_to(ROOT).as_posix(), "reason": "Restored URP previous-version normalization (12 -> 13) only"}]
    return []


def editor_preflight(expected_editor):
    if os.name != "nt":
        raise RuntimeError("Specify a supported Windows development host for Editor execution")
    process = subprocess.run(["powershell.exe", "-NoProfile", "-Command",
                              "ConvertTo-Json -InputObject @(Get-CimInstance Win32_Process -Filter \"Name='Unity.exe'\" | Select-Object ExecutablePath) -Compress"],
                             capture_output=True, text=True, timeout=15)
    if process.returncode:
        raise RuntimeError("Could not determine running Editors; no Editor was launched")
    processes = json.loads(process.stdout)
    if any(not p.get("ExecutablePath") for p in processes):
        raise RuntimeError("A Unity process could not be identified; no Editor was launched")
    if any(is_editor_process(p["ExecutablePath"], expected_editor) for p in processes):
        raise RuntimeError("Close Unity Editors after saving work, then rerun; this tool never closes an existing Editor")


def report_result(task, run_dir):
    if task == "audit":
        value = read_json(run_dir / "inspector-audit.json")
        return value.get("success") is True and value.get("sceneOpenCount") == 5
    if task == "play":
        value = read_json(run_dir / "play-tests.json")
        tests = value.get("tests", [])
        return value.get("success") is True and any(t.get("passed") is True for t in tests) and all(
            t.get("passed") is True or t.get("skipped") is True for t in tests)
    value = read_json(run_dir / "android-build.json")
    apk = Path(value["output"])
    artifact = read_json(run_dir / "apk-artifact.json")
    return value["result"] == "Succeeded" and value["totalErrors"] == 0 and apk.is_file() and digest(apk) == artifact["sha256"]


def run(args):
    started = time.perf_counter()
    run_dir = EVIDENCE / "runs" / (datetime.now().strftime("%Y%m%d-%H%M%S-") + uuid.uuid4().hex[:8])
    run_dir.mkdir(parents=True)
    before = inputs()
    report = {"startedUtc": datetime.now(timezone.utc).isoformat(), "rootProject": str(ROOT), "project": str(PROJECT),
              "task": args.task, "suite": args.suite, "files": before,
              "scope": "Automated checks only; physical Quest input, tracking, readability and usability unverified"}
    write_json(run_dir / "inputs.json", report)
    urp = PROJECT / "Assets/Settings/Mobile_RPAsset.asset"
    urp_before = urp.read_bytes()
    try:
        checks = static_checks(args.protect_baseline)
        report["staticChecks"] = checks
        if not all(c["passed"] for c in checks):
            raise RuntimeError("Static checks failed: " + ", ".join(c["name"] for c in checks if not c["passed"]))
        if args.task != "static":
            version = re.search(r"m_EditorVersion: (\S+)", (PROJECT / "ProjectSettings/ProjectVersion.txt").read_text())[1]
            editor = Path(args.editor) if args.editor else Path(os.environ.get("ProgramFiles", "C:/Program Files")) / f"Unity/Hub/Editor/{version}/Editor/Unity.exe"
            if not editor.is_file():
                raise RuntimeError("Matching Unity Editor missing; pass --editor with its executable path")
            editor_preflight(editor)
            report["command"] = editor_command(editor, run_dir, args.task, args.suite)
            process = subprocess.Popen(report["command"], cwd=PROJECT, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            try:
                report["editorExitCode"] = process.wait(timeout=args.timeout)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait()
                raise RuntimeError("Editor timed out; only the Editor launched by this run was stopped")
            if report["editorExitCode"] != 0 or not report_result(args.task, run_dir):
                raise RuntimeError("Editor verification failed; inspect this run's JSON and editor.log")
        report["success"] = True
    except Exception as error:
        report.update(success=False, error=str(error))
    finally:
        report["restoredAutomaticChanges"] = restore_urp_normalization(urp, urp_before)
        after = inputs()
        report["inputChanges"] = [p for p, sha in before.items() if after.get(p) != sha]
        report["protectedChanges"] = [p for p, sha in before.items() if protected(p) and after.get(p) != sha]
        if report["inputChanges"]:
            report.update(success=False, error="Input files changed during verification; see inputChanges and protectedChanges")
    report["seconds"] = round(time.perf_counter() - started, 3)
    write_json(run_dir / "result.json", report)
    print(json.dumps({k: report.get(k) for k in ("success", "task", "suite", "seconds", "error")}, ensure_ascii=False))
    print("Report: " + str(run_dir / "result.json"))
    return 0 if report["success"] else 1


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="action", required=True)
    sub.add_parser("snapshot", help="Create the immutable before-change hash baseline; refuses overwrite")
    check = sub.add_parser("check", help="Run static checks, plus an optional isolated Editor task")
    check.add_argument("--task", choices=("static", "audit", "play", "build"), default="static")
    check.add_argument("--suite", choices=("all", "score", "physics", "return", "input", "placement"), default="all")
    check.add_argument("--editor")
    check.add_argument("--timeout", type=int, default=1200)
    check.add_argument("--protect-baseline", action="store_true", help="Also enforce the immutable pre-improvement baseline")
    args = parser.parse_args()
    if args.action == "snapshot":
        path = EVIDENCE / "before.json"
        if path.exists():
            parser.error("Baseline already exists; it must not be overwritten")
        write_json(path, {"createdUtc": datetime.now(timezone.utc).isoformat(), "files": inputs()})
        print(path)
        return 0
    if args.timeout <= 0:
        parser.error("--timeout must be positive")
    if args.task != "play" and args.suite != "all":
        parser.error("--suite is only used with --task play")
    return run(args)


if __name__ == "__main__":
    sys.exit(main())
