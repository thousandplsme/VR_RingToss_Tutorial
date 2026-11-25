using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static partial class CourseTests
{
    static void TestScoreBoundaries()
    {
        float radius = Field<float>(score, "centerTolerance");
        float low = Field<float>(score, "minHeight");
        float high = Field<float>(score, "maxHeight");
        foreach (var item in new[] {
            ("radius_boundary", new Vector3(radius, .125f, 0), true),
            ("outside_radius_boundary", new Vector3(radius + .001f, .125f, 0), false),
            ("minimum_height", new Vector3(0, low, 0), true),
            ("below_minimum_height", new Vector3(0, low - .001f, 0), false),
            ("maximum_height", new Vector3(0, high, 0), true),
            ("above_maximum_height", new Vector3(0, high + .001f, 0), false) })
        {
            score.BeginNewPlayer();
            Pose(item.Item2, Quaternion.identity);
            Settle(.51f);
            Assert(item.Item1, (score.Score == 1) == item.Item3);
        }
        foreach (var item in new[] { ("speed_limit", .15f, 0f, true), ("above_speed_limit", .151f, 0f, false),
                                     ("spin_limit", 0f, .5f, true), ("above_spin_limit", 0f, .501f, false) })
        {
            score.BeginNewPlayer();
            Pose(new Vector3(0, .125f, 0), Quaternion.identity, Vector3.right * item.Item2);
            ring.angularVelocity = Vector3.up * item.Item3;
            Settle(.51f);
            Assert(item.Item1, (score.Score == 1) == item.Item4);
        }
        score.BeginNewPlayer();
        Pose(new Vector3(0, .125f, 0), Quaternion.identity);
        Settle(.499f);
        Assert("before_settle_limit", score.Score == 0);
        Settle(.002f);
        Assert("after_settle_limit", score.Score == 1);
    }

    static IEnumerator PlacementRoutine()
        => (IEnumerator)ring.GetComponent<PlaceRingAtStart>().GetType()
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(ring.GetComponent<PlaceRingAtStart>(), null);

    static void TestPlacement()
    {
        ring.useGravity = false;
        Pose(new Vector3(.4f, 1f, 0), Quaternion.identity);
        Vector3 before = ring.position;
        var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
        bool tracked = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool value) && value;
        if (!tracked)
        {
            var waiting = PlacementRoutine();
            bool waitingEveryStep = true;
            for (int i = 0; i < 40; i++) waitingEveryStep &= waiting.MoveNext();
            Assert("placement_waits_without_tracking", waitingEveryStep && ring.position == before);
            (waiting as IDisposable)?.Dispose();
        }
        else Results.Add(new { name = "placement_waits_without_tracking", passed = (bool?)null,
            skipped = true, detail = "A real tracked HMD is connected; no fake tracking result is reported" });

        var handObject = new GameObject("PlacementTestHand");
        var earlyHand = handObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
        earlyHand.interactionManager = manager;
        try
        {
            manager.SelectEnter((IXRSelectInteractor)earlyHand, (IXRSelectInteractable)grab);
            Assert("placement_early_selection_established", grab.isSelected);
            var selected = PlacementRoutine();
            Assert("placement_stops_if_already_held", !selected.MoveNext() && ring.position == before);
            (selected as IDisposable)?.Dispose();
        }
        finally
        {
            manager.SelectExit((IXRSelectInteractor)earlyHand, (IXRSelectInteractable)grab);
            UnityEngine.Object.Destroy(handObject);
        }
    }

    static void TestInput()
    {
        const string layout = "CourseValidationController";
        var previousSettings = InputSystem.settings;
        var testSettings = UnityEngine.Object.Instantiate(previousSettings);
        testSettings.hideFlags = HideFlags.HideAndDontSave;
        testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings = testSettings;
        InputSystem.RegisterLayout(@"{
            ""name"":""CourseValidationController"", ""extend"":""XRController"",
            ""controls"":[
                {""name"":""primaryButton"",""layout"":""Button"",""format"":""FLT"",""usages"":[""PrimaryButton""]},
                {""name"":""secondaryButton"",""layout"":""Button"",""format"":""FLT"",""usages"":[""SecondaryButton""]}]
        }");
        InputDevice device = null;
        try
        {
            device = InputSystem.AddDevice(layout);
            InputSystem.SetDeviceUsage(device, UnityEngine.InputSystem.CommonUsages.RightHand);
            var a = (ButtonControl)device["primaryButton"];
            var b = (ButtonControl)device["secondaryButton"];
            var returnAction = Field<InputAction>(controls, "returnAction");
            var newPlayerAction = Field<InputAction>(controls, "newPlayerAction");
            controls.enabled = true;
            InputSystem.Update();
            Assert("input_actions_enable", returnAction.enabled && newPlayerAction.enabled);
            InputSystem.QueueDeltaStateEvent(a, 1f); InputSystem.Update();
            Assert("right_primary_resolves_return", returnAction.WasPressedThisFrame() && !newPlayerAction.WasPressedThisFrame(),
                new { device.enabled, button = a.ReadValue(), paths = returnAction.controls.Select(c => c.path).ToArray() });
            InputSystem.Update();
            Assert("held_primary_has_no_repeat_edge", !returnAction.WasPressedThisFrame());
            InputSystem.QueueDeltaStateEvent(a, 0f); InputSystem.Update();
            InputSystem.QueueDeltaStateEvent(a, 1f); InputSystem.Update();
            Assert("primary_repress_has_edge", returnAction.WasPressedThisFrame());
            InputSystem.QueueDeltaStateEvent(a, 0f);
            InputSystem.QueueDeltaStateEvent(b, 1f); InputSystem.Update();
            Assert("right_secondary_resolves_new_player", newPlayerAction.WasPressedThisFrame() && !returnAction.WasPressedThisFrame());
            controls.enabled = false;
            InputSystem.QueueDeltaStateEvent(b, 0f); InputSystem.Update();
            InputSystem.QueueDeltaStateEvent(a, 1f); InputSystem.Update();
            Assert("disabled_input_actions_ignore_button", !returnAction.enabled && !newPlayerAction.enabled && !returnAction.WasPressedThisFrame());
        }
        finally
        {
            controls.enabled = false;
            if (device != null) InputSystem.RemoveDevice(device);
            InputSystem.RemoveLayout(layout);
            InputSystem.settings = previousSettings;
            UnityEngine.Object.DestroyImmediate(testSettings);
        }
    }
}
