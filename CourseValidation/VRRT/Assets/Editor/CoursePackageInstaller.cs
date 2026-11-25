using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class CoursePackageInstaller
{
    private static AddAndRemoveRequest request;
    private static double deadline;
    public static void Install()
    {
        request = Client.AddAndRemove(new[] {
            "com.unity.inputsystem@1.18.0",
            "com.unity.xr.management@4.5.3",
            "com.unity.xr.openxr@1.16.1",
            "com.unity.xr.interaction.toolkit@3.3.1",
            "com.unity.pipeline@0.7.0-exp.1"
        });
        deadline = EditorApplication.timeSinceStartup + 600;
        EditorApplication.update += Poll;
    }

    private static void Poll()
    {
        if (!request.IsCompleted)
        {
            if (EditorApplication.timeSinceStartup > deadline)
            {
                Debug.LogError("Course packages timed out.");
                EditorApplication.Exit(2);
            }
            return;
        }
        EditorApplication.update -= Poll;
        if (request.Status == StatusCode.Success)
        {
            Debug.Log("Course packages resolved.");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError(request.Error.message);
            EditorApplication.Exit(1);
        }
    }
}
