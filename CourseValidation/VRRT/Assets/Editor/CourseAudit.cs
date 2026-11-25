using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using Unity.XR.CoreUtils;
using TMPro;

public static class CourseAudit
{
    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    static UnityEngine.Object Reference(UnityEngine.Object owner, string field)
    {
        var value = new SerializedObject(owner).FindProperty(field).objectReferenceValue;
        Require(value != null, owner.name + "." + field + " missing");
        return value;
    }
    static Dictionary<string, float> Floats(UnityEngine.Object owner, params string[] fields)
        => fields.ToDictionary(f => f, f => new SerializedObject(owner).FindProperty(f).floatValue);
    public static void Run()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var records = new List<object>();
        CourseScenes.InspectSavedScenes((scene, name) =>
        {
            var roots = scene.GetRootGameObjects();
            var ring = GameObject.Find("Ring");
            var origin = UnityEngine.Object.FindFirstObjectByType<XROrigin>();
            var actions = origin.GetComponent<InputActionManager>();
            Require(origin.Camera != null && origin.RequestedTrackingOriginMode == XROrigin.TrackingOriginMode.Floor, "XR camera/origin");
            Require(actions.actionAssets.Count == 1 && actions.actionAssets[0] != null, "Input Action Manager reference");
            Require(!origin.transform.Find("Locomotion").gameObject.activeSelf, "Locomotion enabled");
            Require(ring.transform.position == new Vector3(0, 1.2f, .5f) && ring.transform.localScale == Vector3.one, "Ring transform");
            var placement = ring.GetComponent<PlaceRingAtStart>();
            Require(placement != null && placement.enabled && Reference(placement, "head") == origin.Camera.transform, "Placement camera");
            Require(ring.GetComponentsInChildren<Rigidbody>().Length == 1, "Compound Rigidbody count");
            var capsules = ring.GetComponentsInChildren<CapsuleCollider>();
            Require(capsules.Length == 16 && capsules.All(c => c.enabled && !c.isTrigger), "Ring collider state");
            Require(ring.GetComponent<MeshRenderer>().sharedMaterial.shader != null, "Ring shader");
            var grab = ring.GetComponent<XRGrabInteractable>();
            Require(grab.startingSingleGrabTransformers.Count == 1 && grab.startingSingleGrabTransformers[0] != null && !grab.addDefaultGrabTransformers, "Grab transformer");
            var target = GameObject.Find("Target");
            var score = target == null ? null : target.GetComponent<RingScore>();
            int chapter = int.Parse(name.Substring(0, 2));
            Require((target != null) == (chapter >= 3), "Chapter target presence: " + name);
            Require((score != null) == (chapter >= 4), "Chapter score presence: " + name);
            if (target != null)
            {
                Require(target.transform.position == new Vector3(0, 0, 1.5f), "Target position");
                Require(target.transform.Find("Base").localScale == new Vector3(.5f, .1f, .5f), "Base size");
                Require(target.transform.Find("Pole").localScale == new Vector3(.06f, .3f, .06f), "Pole size");
            }
            if (score != null) Require(score.enabled && Reference(score, "ring") == ring.GetComponent<Rigidbody>(), "Score ring");
            var initial = Floats(placement, "distance", "belowEyes");
            Require(Mathf.Approximately(initial["distance"], .45f) && Mathf.Approximately(initial["belowEyes"], .35f), "Placement values");
            var controls = ring.GetComponent<RingPracticeControls>();
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (name.StartsWith("05"))
            {
                Require(controls != null && controls.enabled && Reference(controls, "head") == origin.Camera.transform && Reference(controls, "target") == score, "Return references");
                var returned = Floats(controls, "distance", "belowEyes");
                Require(returned.All(pair => Mathf.Approximately(pair.Value, initial[pair.Key])), "Placement/return values differ");
                Require(canvas != null && canvas.renderMode == RenderMode.WorldSpace, "World space canvas");
                var display = canvas.GetComponent<RingScoreDisplay>();
                Require(Reference(display, "target") == score && Reference(display, "label") == GameObject.Find("ScoreText").GetComponent<TMP_Text>(), "Display references");
                Require(GameObject.Find("ScoreText").GetComponent<TMP_Text>().font != null, "TMP font");
            }
            else Require(controls == null && canvas == null, "Later chapter components leaked");
            Require(!scene.isDirty, "Dirty saved scene");
            records.Add(new { scene = scene.path, camera = origin.Camera.name, trackingOrigin = origin.RequestedTrackingOriginMode.ToString(),
                input = AssetDatabase.GetAssetPath(actions.actionAssets[0]), placement = Floats(placement, "distance", "belowEyes"),
                movement = grab.movementType.ToString(), grab.throwOnDetach, grab.forceGravityOnDetach, grab.throwSmoothingDuration,
                grab.throwVelocityScale, grab.throwAngularVelocityScale, grab.useDynamicAttach, grab.attachEaseInTime,
                score = score == null ? null : Floats(score, "centerTolerance", "minHeight", "maxHeight", "uprightDot", "maxSpeed", "maxSpin", "settleSeconds"),
                controls = controls == null ? null : Floats(controls, "distance", "belowEyes"),
                texts = canvas == null ? null : canvas.GetComponentsInChildren<TMP_Text>().Select(t => new { t.name, t.text, t.fontSize, font = t.font.name, t.raycastTarget }).ToArray()
            });
        });
        var xr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        var general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        File.WriteAllText(CourseScenes.ReportFolder + "/inspector-audit.json", Newtonsoft.Json.JsonConvert.SerializeObject(new {
            success = true, utc = DateTime.UtcNow.ToString("o"), project = Application.dataPath,
            sceneOpenCount = records.Count, seconds = timer.Elapsed.TotalSeconds,
            unity = Application.unityVersion, activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
            android = new { initializedOnStart = general.InitManagerOnStart, loaders = general.Manager.activeLoaders.Select(l => l.GetType().Name).ToArray(),
                features = xr.GetFeatures().Where(f => f.enabled).Select(f => f.GetType().Name).ToArray(),
                minSdk = PlayerSettings.Android.minSdkVersion.ToString(), architectures = PlayerSettings.Android.targetArchitectures.ToString(),
                graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Select(g => g.ToString()).ToArray() },
            scenes = records }, Newtonsoft.Json.Formatting.Indented));
        Debug.Log("COURSE_INSPECTOR_AUDIT_PASSED");
    }
}
