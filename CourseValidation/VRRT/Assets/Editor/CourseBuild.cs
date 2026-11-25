using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features;

public static class CourseBuild
{
    public static void Android()
    {
        CourseValidationRunner.RequireIdleSavedProject();
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            throw new Exception("Switch the validation project to Android before building");
        AssetDatabase.DesiredWorkerCount = 1;
        CourseAudit.Run();
        var issues = new List<OpenXRFeature.ValidationRule>();
        OpenXRProjectValidation.GetCurrentValidationIssues(issues, BuildTargetGroup.Android);
        File.WriteAllText(CourseScenes.ReportFolder + "/xr-validation.json", Newtonsoft.Json.JsonConvert.SerializeObject(
            issues.Select(i => new { i.message, i.error }), Newtonsoft.Json.Formatting.Indented));
        if (issues.Any(i => i.error)) throw new Exception("Android OpenXR validation errors: see xr-validation.json");
        string output = Path.Combine(CourseScenes.ReportFolder, "05_RingToss.apk");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/05_RingToss.unity" },
            locationPathName = output, target = BuildTarget.Android,
            options = BuildOptions.None
        });
        File.WriteAllText(CourseScenes.ReportFolder + "/android-build.json", Newtonsoft.Json.JsonConvert.SerializeObject(new {
            utc = DateTime.UtcNow.ToString("o"), project = Application.dataPath, unity = Application.unityVersion,
            result = report.summary.result.ToString(), report.summary.totalErrors, report.summary.totalWarnings,
            report.summary.totalSize, seconds = report.summary.totalTime.TotalSeconds,
            output = report.summary.outputPath,
            messages = report.steps.SelectMany(s => s.messages).Where(m => m.type == LogType.Error || m.type == LogType.Exception).Select(m => m.content).ToArray()
        }, Newtonsoft.Json.Formatting.Indented));
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Android build failed");
        using (var stream = File.OpenRead(output))
        using (var sha = System.Security.Cryptography.SHA256.Create())
            File.WriteAllText(CourseScenes.ReportFolder + "/apk-artifact.json", Newtonsoft.Json.JsonConvert.SerializeObject(new {
                name = Path.GetFileName(output), bytes = stream.Length,
                sha256 = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""), deviceTested = false
            }, Newtonsoft.Json.Formatting.Indented));
        Debug.Log("COURSE_ANDROID_BUILD_PASSED");
    }
}
