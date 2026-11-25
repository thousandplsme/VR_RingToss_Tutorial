using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[InitializeOnLoad]
public static partial class CourseTests
{
    const string Key = "CourseTests.running";
    static readonly List<object> Results = new List<object>();
    static readonly List<string> Errors = new List<string>();
    static int stage, releasedFrame;
    static Rigidbody ring;
    static RingScore score;
    static RingPracticeControls controls;
    static XRGrabInteractable grab;
    static XRDirectInteractor hand;
    static XRInteractionManager manager;
    static Transform head;
    static PhysicsScene physics;
    static double deadline;
    static bool assertionsPassed = true;
    static string Suite => SessionState.GetString("CourseTests.suite", "all");
    [Serializable] class SavedSetup { public SceneSetup[] scenes; }

    static CourseTests()
    {
        EditorApplication.update += Tick;
        Application.logMessageReceived += (message, trace, type) =>
        {
            if (SessionState.GetBool(Key, false) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                Errors.Add(message);
        };
    }

    public static void Run()
    {
        CourseValidationRunner.RequireIdleSavedProject();
        if (SessionState.GetBool(Key, false)) throw new Exception("A course test is already running");
        Results.Clear(); Errors.Clear(); stage = 0; deadline = 0; assertionsPassed = true;
        string suite = CourseValidationRunner.Argument("-courseSuite", "all");
        if (Array.IndexOf(new[] { "all", "score", "physics", "return", "input", "placement" }, suite) < 0)
            throw new Exception("Unknown test suite: " + suite);
        SessionState.SetString("CourseTests.suite", suite);
        SessionState.SetBool("CourseTests.enteredPlay", false);
        SessionState.SetString("CourseTests.setup", JsonUtility.ToJson(new SavedSetup { scenes = EditorSceneManager.GetSceneManagerSetup() }));
        AssetDatabase.DesiredWorkerCount = 1;
        EditorSceneManager.OpenScene("Assets/Scenes/05_RingToss.unity");
        SessionState.SetBool(Key, true);
        SessionState.SetString("CourseTests.result", "");
        EditorApplication.EnterPlaymode();
    }

    static void Assert(string name, bool ok, object detail = null)
    {
        Results.Add(new { name, passed = ok, detail });
        if (!ok) assertionsPassed = false;
    }

    static T Field<T>(object obj, string name) => (T)obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
    static void Pose(Vector3 local, Quaternion rotation, Vector3 speed = default)
    {
        ring.position = score.transform.TransformPoint(local);
        ring.rotation = rotation;
        ring.linearVelocity = speed;
        ring.angularVelocity = Vector3.zero;
        Physics.SyncTransforms();
    }
    static void Settle(float seconds) => score.CheckScore(seconds);

    static void Tick()
    {
        if (!SessionState.GetBool(Key, false)) return;
        if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 120;
        if (!EditorApplication.isPlaying)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string result = SessionState.GetString("CourseTests.result", "");
            if (result.Length == 0)
            {
                if (SessionState.GetBool("CourseTests.enteredPlay", false)) Finish("Play stopped before test completion");
                else if (EditorApplication.timeSinceStartup > deadline) Finish("Play did not start before timeout");
                return;
            }
            SessionState.SetBool(Key, false);
            string setup = SessionState.GetString("CourseTests.setup", "");
            if (setup.Length > 0) CourseValidationRunner.RestoreSetup(JsonUtility.FromJson<SavedSetup>(setup).scenes);
            if (Application.isBatchMode) EditorApplication.Exit(result == "pass" ? 0 : 2);
            return;
        }
        if (SessionState.GetString("CourseTests.result", "").Length > 0) return;
        SessionState.SetBool("CourseTests.enteredPlay", true);
        if (EditorApplication.timeSinceStartup > deadline) { Finish("Play test timed out"); return; }
        if (Time.frameCount < 8) return;
        try
        {
            if (stage == 0)
            {
                PrepareAndTest();
                if (Suite != "all" && Suite != "return")
                {
                    Assert("no_runtime_errors", Errors.Count == 0, Errors.ToArray());
                    Finish(null); return;
                }
                stage = 1; releasedFrame = Time.frameCount; return;
            }
            if (stage == 1 && Time.frameCount > releasedFrame + 2)
            {
                Assert("return_after_release_grace", controls.TryReturn(false));
                Assert("return_position", Vector3.Distance(ring.position, head.position + Vector3.forward * .45f - Vector3.up * .35f) < .001f, ring.position.ToString());
                Assert("return_stops_motion", !ring.useGravity && ring.linearVelocity.sqrMagnitude < .00001f && ring.angularVelocity.sqrMagnitude < .00001f);
                Pose(new Vector3(0, .125f, 0), Quaternion.identity);
                Settle(.51f);
                Assert("next_throw_can_score", score.Score == 2, score.Score);
                stage = 2; releasedFrame = Time.frameCount; return;
            }
            if (stage == 2 && Time.frameCount > releasedFrame + 1)
            {
                Assert("score_display_updates_in_play", GameObject.Find("ScoreText").GetComponent<TMP_Text>().text == "Score: 2");
                Assert("new_player_resets_score", controls.TryReturn(true) && score.Score == 0);
                head.rotation = Quaternion.Euler(-90, 0, 0);
                Assert("vertical_head_rejects_return", !controls.TryReturn(false));
                head.rotation = Quaternion.identity;
                Assert("no_runtime_errors", Errors.Count == 0, Errors.ToArray());
                Assert("play_frames_advanced", Time.frameCount > 8, Time.frameCount);
                Finish(null);
            }
        }
        catch (Exception exception) { Finish(exception.ToString()); }
    }

    static void PrepareAndTest()
    {
        ring = GameObject.Find("Ring").GetComponent<Rigidbody>();
        score = GameObject.Find("Target").GetComponent<RingScore>();
        controls = ring.GetComponent<RingPracticeControls>();
        grab = ring.GetComponent<XRGrabInteractable>();
        manager = grab.interactionManager;
        head = Field<Transform>(controls, "head");
        ring.GetComponent<PlaceRingAtStart>().StopAllCoroutines();
        controls.enabled = false;
        score.enabled = false;
        head.SetPositionAndRotation(new Vector3(0, 1.6f, 0), Quaternion.identity);

        var local = SceneManager.CreateScene("IsolatedPhysics", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physics = local.GetPhysicsScene();
        SceneManager.MoveGameObjectToScene(ring.gameObject, local);
        SceneManager.MoveGameObjectToScene(score.gameObject, local);
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "TestFloor";
        floor.transform.position = new Vector3(0, -.05f, 0);
        floor.transform.localScale = new Vector3(4, .1f, 4);
        SceneManager.MoveGameObjectToScene(floor, local);

        Case("score", TestScoring);
        Case("physics", TestPhysics);
        Case("placement", TestPlacement);
        Case("input", TestInput);
        Case("return", TestReturnGuards);
    }

    static void Case(string suite, Action test)
    {
        if (Suite != "all" && Suite != suite) return;
        try { test(); }
        catch (Exception error) { Assert(suite + "_unexpected_exception", false, error.ToString()); }
    }

    static void TestScoring()
    {

        Pose(new Vector3(0, .125f, 0), Quaternion.identity);
        Settle(.3f);
        Assert("settling_delay", score.Score == 0);
        Pose(new Vector3(.4f, .125f, 0), Quaternion.identity);
        Settle(.3f);
        Pose(new Vector3(0, .125f, 0), Quaternion.identity);
        Settle(.3f);
        Assert("interruption_resets_timer", score.Score == 0);
        foreach (var item in new[] {
            ("outside", new Vector3(.4f,.125f,0), Quaternion.identity, Vector3.zero),
            ("too_high", new Vector3(0,.4f,0), Quaternion.identity, Vector3.zero),
            ("tilted", new Vector3(0,.125f,0), Quaternion.Euler(60,0,0), Vector3.zero),
            ("moving_fast", new Vector3(0,.125f,0), Quaternion.identity, Vector3.right) })
        {
            score.BeginNewPlayer();
            Pose(item.Item2, item.Item3, item.Item4); Settle(1);
            Assert("reject_" + item.Item1, score.Score == 0);
        }
        score.BeginNewPlayer();
        Pose(new Vector3(0, .125f, 0), Quaternion.identity);
        ring.angularVelocity = Vector3.up;
        Settle(1);
        Assert("reject_spinning", score.Score == 0);
        TestScoreBoundaries();
    }

    static void TestPhysics()
    {
        score.BeginNewPlayer();
        Pose(new Vector3(0, 1f, 0), Quaternion.identity);
        ring.useGravity = true;
        for (int i = 0; i < 250; i++) { physics.Simulate(.02f); Settle(.02f); }
        Assert("physical_drop_through_hole_scores_once", score.Score == 1, new { score.Score, position = ring.position.ToString(), speed = ring.linearVelocity.magnitude });
        for (int i = 0; i < 100; i++) { physics.Simulate(.02f); Settle(.02f); }
        Assert("resting_ring_no_duplicate", score.Score == 1);

        score.BeginNewPlayer();
        Pose(new Vector3(.4f, 1f, 0), Quaternion.identity);
        for (int i = 0; i < 250; i++) { physics.Simulate(.02f); Settle(.02f); }
        Assert("physical_miss_does_not_score", score.Score == 0, ring.position.ToString());
        Assert("floor_collision", ring.position.y > -.01f, ring.position.y);
    }

    static void TestReturnGuards()
    {
        ring.useGravity = false;
        score.BeginNewPlayer();
        Pose(new Vector3(0, .125f, 0), Quaternion.identity);
        var handObject = new GameObject("TestHand");
        hand = handObject.AddComponent<XRDirectInteractor>();
        hand.interactionManager = manager;
        manager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)grab);
        Assert("xr_selection_established", grab.isSelected);
        Settle(1);
        Assert("held_ring_no_score", score.Score == 0);
        Assert("held_ring_cannot_return", !controls.TryReturn(false) && !controls.TryReturn(true));
        manager.SelectExit((IXRSelectInteractor)hand, (IXRSelectInteractable)grab);
        Assert("release_frame_cannot_return", !controls.TryReturn(false));
        int released = Field<int>(controls, "lastReleaseFrame");
        controls.GetType().GetField("lastReleaseFrame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controls, Time.frameCount - 1);
        Assert("next_release_frame_cannot_return", !controls.TryReturn(false));
        controls.GetType().GetField("lastReleaseFrame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controls, released);
        Pose(new Vector3(0, .125f, 0), Quaternion.identity);
        Settle(.51f);
        Assert("unheld_still_ring_scores", score.Score == 1);
    }

    static void Finish(string failure)
    {
        if (failure == null && !assertionsPassed) failure = "One or more assertions failed; remaining cases were still executed";
        SessionState.SetString("CourseTests.result", failure == null ? "pass" : "fail");
        File.WriteAllText(CourseScenes.ReportFolder + "/play-tests.json", Newtonsoft.Json.JsonConvert.SerializeObject(new {
            success = failure == null, failure, suite = Suite, utc = DateTime.UtcNow.ToString("o"),
            project = Application.dataPath, unity = Application.unityVersion, frames = Time.frameCount,
            tests = Results, errors = Errors, scope = "Editor Play + isolated PhysX; Quest tracking, physical buttons and visual readability not verified"
        }, Newtonsoft.Json.Formatting.Indented));
        Debug.Log(failure == null ? "COURSE_PLAY_TESTS_PASSED" : failure);
        if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.ExitPlaymode();
    }
}
