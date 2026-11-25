using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Developer entry points only. Never installs packages, generates scenes, or closes an interactive Editor.
public static class CourseValidationRunner
{
    public static readonly string[] SceneNames = { "01_Grab_Complete", "02_Throw", "03_Target", "04_Score", "05_RingToss" };
    const string ReportKey = "CourseValidation.reportDirectory";

    public static string Argument(string name, string fallback = "")
    {
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
    }

    public static string ReportDirectory
    {
        get
        {
            string path = SessionState.GetString(ReportKey, "");
            if (path.Length == 0) BeginReport();
            return SessionState.GetString(ReportKey, "");
        }
    }

    public static void BeginReport()
    {
        string parent = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../Docs/CourseValidation/Efficiency/runs"));
        string requested = Argument("-courseReportDirectory");
        string path = requested.Length == 0
            ? Path.Combine(parent, DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8))
            : Path.GetFullPath(requested);
        if (!path.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new Exception("Report directory must be inside Docs/CourseValidation/Efficiency/runs");
        Directory.CreateDirectory(path);
        SessionState.SetString(ReportKey, path);
        if (!File.Exists(Path.Combine(path, "inputs.json")))
        {
            string project = Path.GetDirectoryName(Application.dataPath);
            var files = new SortedDictionary<string, string>();
            using (var sha = SHA256.Create())
                foreach (string area in new[] { "Assets", "Packages", "ProjectSettings" })
                    foreach (string file in Directory.EnumerateFiles(Path.Combine(project, area), "*", SearchOption.AllDirectories))
                        using (var stream = File.OpenRead(file))
                            files[file.Substring(project.Length + 1).Replace('\\', '/')] = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
            File.WriteAllText(Path.Combine(path, "inputs.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new {
                startedUtc = DateTime.UtcNow.ToString("o"), project, unity = Application.unityVersion, files,
                scope = "Editor checks only; physical Quest input and usability unverified"
            }, Newtonsoft.Json.Formatting.Indented));
        }
    }

    public static void RestoreSetup(SceneSetup[] setup)
    {
        if (setup != null && setup.Length > 0 && setup.All(s => !string.IsNullOrEmpty(s.path)))
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    public static void RequireIdleSavedProject()
    {
        if (!Application.dataPath.Replace('\\', '/').EndsWith("/CourseValidation/VRRT/Assets", StringComparison.OrdinalIgnoreCase))
            throw new Exception("This operation is only for CourseValidation/VRRT");
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            throw new Exception("Wait until the Editor is idle and Play is stopped");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isDirty || (string.IsNullOrEmpty(scene.path) && scene.rootCount > 0))
                throw new Exception("Save the open scenes before validation; no scene was changed");
        }
    }

    public static void Run()
    {
        try
        {
            RequireIdleSavedProject();
            BeginReport();
            switch (Argument("-courseTask", "audit"))
            {
                case "audit": CourseAudit.Run(); break;
                case "play": CourseTests.Run(); return; // Asynchronous; CourseTests owns completion.
                case "build": CourseBuild.Android(); break;
                default: throw new Exception("Unknown course task");
            }
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            if (Application.isBatchMode) EditorApplication.Exit(2);
            else throw;
        }
    }

    [MenuItem("Tools/Ring Toss/Validate Saved Chapters")]
    static void AuditMenu() { RequireIdleSavedProject(); BeginReport(); CourseAudit.Run(); }

    [MenuItem("Tools/Ring Toss/Play Tests/All")]
    static void PlayMenu() { RequireIdleSavedProject(); BeginReport(); CourseTests.Run(); }
}
