using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using Unity.XR.CoreUtils;
using TMPro;

public static class CourseScenes
{
    const string Samples = "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets";
    public static string ReportFolder => CourseValidationRunner.ReportDirectory;
    public static void SetReference(UnityEngine.Object obj, string name, UnityEngine.Object value)
    {
        var so = new SerializedObject(obj);
        var property = so.FindProperty(name);
        if (property == null) throw new Exception("Missing field: " + name);
        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void ConfigureAndroid()
    {
        Directory.CreateDirectory("Assets/XR/Settings");
        if (!EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out var perTarget))
        {
            perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(perTarget, "Assets/XR/Settings/CourseXRSettings.asset");
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
        }
        if (!perTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
            perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        var general = perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
        general.InitManagerOnStart = true;
        if (!XRPackageMetadataStore.AssignLoader(general.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android))
            throw new Exception("OpenXR loader assignment failed");
        var packageType = TypeCache.GetTypesDerivedFrom<ScriptableObject>().Single(t => t.FullName == "UnityEditor.XR.OpenXR.OpenXRPackageSettings");
        packageType.GetMethod("GetOrCreateInstance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Invoke(null, null);
        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        var wanted = new[] { "OculusTouchControllerProfile", "MetaQuestTouchProControllerProfile", "MetaQuestTouchPlusControllerProfile", "MetaQuestFeature" };
        foreach (var name in wanted)
        {
            var feature = settings.GetFeatures().Single(f => f.GetType().Name == name);
            feature.enabled = true;
            EditorUtility.SetDirty(feature);
        }
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.Vulkan });
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.vrclub.ringtoss.validation");
        var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        player.FindProperty("activeInputHandler").intValue = 1;
        player.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(general);
        EditorUtility.SetDirty(perTarget);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        return go;
    }

    static TextMeshProUGUI Text(string name, Transform parent, string value, float y, float height, float size, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition3D = new Vector3(0, y, 0);
        rt.sizeDelta = new Vector2(600, height);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    public static void Build()
    {
        CourseValidationRunner.RequireIdleSavedProject();
        var names = CourseValidationRunner.SceneNames;
        foreach (var name in names)
            if (File.Exists("Assets/Scenes/" + name + ".unity")) throw new Exception("Refusing to overwrite " + name);
        AssetDatabase.DesiredWorkerCount = 1;
        ConfigureAndroid();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        UnityEngine.Object.DestroyImmediate(GameObject.Find("Main Camera"));
        Primitive(PrimitiveType.Cube, "Floor", null, new Vector3(0, -0.05f, 0), new Vector3(4, 0.1f, 4));
        var rigAsset = AssetDatabase.LoadAssetAtPath<GameObject>(Samples + "/Prefabs/XR Origin (XR Rig).prefab");
        if (rigAsset == null) throw new Exception("Starter Assets 3.3.1 rig missing");
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigAsset);
        rig.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        rig.transform.localScale = Vector3.one;
        var origin = rig.GetComponent<XROrigin>();
        origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        var locomotion = rig.transform.Find("Locomotion");
        if (locomotion == null) throw new Exception("Locomotion child missing");
        locomotion.gameObject.SetActive(false);
        var actions = rig.GetComponent<InputActionManager>();
        if (actions == null) actions = rig.AddComponent<InputActionManager>();
        if (actions.actionAssets == null || actions.actionAssets.Count == 0)
            actions.actionAssets = new List<InputActionAsset> { AssetDatabase.LoadAssetAtPath<InputActionAsset>(Samples + "/XRI Default Input Actions.inputactions") };
        var manager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
        if (manager == null) manager = new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
        var shape = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RingTossMaterials/RingShape.prefab");
        if (shape == null || shape.GetComponent<MeshFilter>().sharedMesh == null || shape.GetComponent<MeshRenderer>().sharedMaterial == null)
            throw new Exception("Package mesh/material dependency missing");
        var ring = (GameObject)PrefabUtility.InstantiatePrefab(shape);
        ring.name = "Ring";
        ring.transform.position = new Vector3(0, 1.2f, 0.5f);
        var body = ring.AddComponent<Rigidbody>();
        body.mass = 0.15f;
        body.useGravity = false;
        body.isKinematic = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        var grab = ring.AddComponent<XRGrabInteractable>();
        grab.interactionManager = manager;
        grab.selectMode = InteractableSelectMode.Single;
        grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grab.trackPosition = grab.trackRotation = true;
        grab.useDynamicAttach = true;
        grab.attachEaseInTime = 0.15f;
        grab.throwOnDetach = grab.forceGravityOnDetach = false;
        grab.throwSmoothingDuration = 0.25f;
        grab.throwVelocityScale = grab.throwAngularVelocityScale = 1f;
        grab.colliders.Clear();
        grab.colliders.AddRange(ring.GetComponentsInChildren<Collider>().OrderBy(c => c.name));
        var transformer = ring.AddComponent<XRGeneralGrabTransformer>();
        transformer.allowOneHandedScaling = transformer.allowTwoHandedScaling = false;
        grab.addDefaultGrabTransformers = false;
        grab.startingSingleGrabTransformers.Clear();
        grab.startingSingleGrabTransformers.Add(transformer);
        var placement = ring.AddComponent<PlaceRingAtStart>();
        SetReference(placement, "head", origin.Camera.transform);
        Save(scene, names[0]);
        grab.throwOnDetach = grab.forceGravityOnDetach = true;
        Save(scene, names[1]);

        var target = new GameObject("Target");
        target.transform.position = new Vector3(0, 0, 1.5f);
        Primitive(PrimitiveType.Cube, "Base", target.transform, new Vector3(0, 0.05f, 0), new Vector3(0.5f, 0.1f, 0.5f));
        Primitive(PrimitiveType.Cylinder, "Pole", target.transform, new Vector3(0, 0.4f, 0), new Vector3(0.06f, 0.3f, 0.06f));
        Save(scene, names[2]);
        var score = target.AddComponent<RingScore>();
        SetReference(score, "ring", body);
        Save(scene, names[3]);

        var canvasGo = new GameObject("ScoreCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = origin.Camera;
        var rect = canvasGo.GetComponent<RectTransform>();
        rect.position = new Vector3(0, 1.35f, 2);
        rect.sizeDelta = new Vector2(600, 200);
        rect.localScale = Vector3.one * 0.0025f;
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null) throw new Exception("TMP Essentials font missing");
        var label = Text("ScoreText", rect, "Score: 0", 40, 100, 48, font);
        Text("ControlsText", rect, "A: Return   B: New Player", -45, 60, 24, font);
        var display = canvasGo.AddComponent<RingScoreDisplay>();
        SetReference(display, "target", score);
        SetReference(display, "label", label);
        var controls = ring.AddComponent<RingPracticeControls>();
        SetReference(controls, "head", origin.Camera.transform);
        SetReference(controls, "target", score);
        Save(scene, names[4]);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/05_RingToss.unity", true) };
        AssetDatabase.SaveAssets();
        Verify();
    }

    static void Save(Scene scene, string name)
    {
        if (!EditorSceneManager.SaveScene(scene, "Assets/Scenes/" + name + ".unity")) throw new Exception("Scene save failed");
    }

    public static void Verify()
        => InspectSavedScenes(null);

    public static void InspectSavedScenes(Action<Scene, string> inspect)
    {
        CourseValidationRunner.RequireIdleSavedProject();
        var setup = EditorSceneManager.GetSceneManagerSetup();
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var records = new List<object>();
        try
        {
            foreach (var name in CourseValidationRunner.SceneNames)
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
                var objects = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
                var ring = GameObject.Find("Ring");
                if (ring == null) throw new Exception("Ring missing: " + name);
                var grab = ring.GetComponent<XRGrabInteractable>();
                var body = ring.GetComponent<Rigidbody>();
                int missing = objects.Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                bool expectedThrow = !name.StartsWith("01");
                if (missing != 0 || grab == null || body == null || !grab.enabled || grab.colliders.Count != 16 || grab.colliders.Any(c => c == null) || grab.throwOnDetach != expectedThrow || grab.forceGravityOnDetach != expectedThrow || body.useGravity || body.isKinematic)
                    throw new Exception("Scene verification failed: " + name);
                records.Add(new { path = scene.path, saved = !scene.isDirty, missingScripts = missing,
                    roots = scene.GetRootGameObjects().Select(g => g.name).ToArray(),
                    colliders = grab.colliders.Select(c => c.name).ToArray(), body.mass, body.useGravity, body.isKinematic,
                    interpolation = body.interpolation.ToString(), collisionDetection = body.collisionDetectionMode.ToString(),
                    grab.throwOnDetach, grab.forceGravityOnDetach, grab.throwVelocityScale,
                    components = ring.GetComponents<Component>().Select(c => c.GetType().Name).ToArray(),
                    mesh = AssetDatabase.GetAssetPath(ring.GetComponent<MeshFilter>().sharedMesh),
                    material = AssetDatabase.GetAssetPath(ring.GetComponent<MeshRenderer>().sharedMaterial) });
                inspect?.Invoke(scene, name);
            }
            File.WriteAllText(ReportFolder + "/scenes.json", Newtonsoft.Json.JsonConvert.SerializeObject(new {
                success = true, utc = DateTime.UtcNow.ToString("o"), project = Application.dataPath,
                sceneOpenCount = records.Count, seconds = timer.Elapsed.TotalSeconds, scenes = records
            }, Newtonsoft.Json.Formatting.Indented));
            Debug.Log("COURSE_SCENES_VERIFIED");
        }
        finally { CourseValidationRunner.RestoreSetup(setup); }
    }
}
