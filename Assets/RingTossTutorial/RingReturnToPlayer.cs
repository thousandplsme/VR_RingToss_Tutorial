using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

/// <summary>Right primary button (Quest A) returns the existing, unheld ring once per press.</summary>
[RequireComponent(typeof(RingInitialPlacement))]
public sealed class RingReturnToPlayer : MonoBehaviour
{
    private InputAction returnAction;
    private RingInitialPlacement placement;

    private void Awake()
    {
        placement = GetComponent<RingInitialPlacement>();
        returnAction = new InputAction("Return Ring", InputActionType.Button,
            "<XRController>{RightHand}/{PrimaryButton}");
    }

    private void OnEnable() => returnAction.Enable();
    private void OnDisable() => returnAction.Disable();
    private void OnDestroy() => returnAction.Dispose();

    private void Update()
    {
        if (!returnAction.WasPressedThisFrame()) return;
        var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        // Do not return to an old head pose when tracking is unavailable.
        if (!head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) || !tracked) return;
        placement.TryPlaceInFront();
    }
}
