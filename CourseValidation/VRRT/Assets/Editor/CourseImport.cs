using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public static class CourseImport
{
    static double deadline;
    static bool fontRequested;
    public static void Run()
    {
        AssetDatabase.DesiredWorkerCount = 1;
        var sample = Sample.FindByPackage("com.unity.xr.interaction.toolkit", "3.3.1")
            .Single(s => s.displayName == "Starter Assets");
        if (!sample.isImported && !sample.Import()) throw new System.Exception("Starter Assets import failed");
        AssetDatabase.ImportPackage(Path.GetFullPath("../../Docs/CourseValidation/Distribution/RingShape_v2.unitypackage"), false);
        deadline = EditorApplication.timeSinceStartup + 300;
        EditorApplication.update += Poll;
    }

    static void Poll()
    {
        if (EditorApplication.timeSinceStartup > deadline)
        {
            Debug.LogError("COURSE_IMPORT_TIMEOUT");
            EditorApplication.Exit(2);
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RingTossMaterials/RingShape.prefab") == null) return;
        if (!fontRequested)
        {
            fontRequested = true;
            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset") == null) return;
        EditorApplication.update -= Poll;
        Debug.Log("COURSE_IMPORT_COMPLETE_ASSETS_VERIFIED");
        EditorApplication.Exit(0);
    }
}
