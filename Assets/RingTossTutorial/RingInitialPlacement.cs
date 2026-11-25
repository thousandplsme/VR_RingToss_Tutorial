using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Places the ring once in front of the first stable tracked head pose.</summary>
[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public sealed class RingInitialPlacement : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private float forwardDistance = 0.45f;
    [SerializeField] private float belowEyes = 0.35f;
    private float trackedTime;
    private bool placed;
    private Rigidbody body;
    private XRGrabInteractable grab;
    private int lastReleaseFrame = -10;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(OnReleased);
    }

    private void OnReleased(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
        => lastReleaseFrame = Time.frameCount;

    private void OnDestroy()
    {
        if (grab != null) grab.selectExited.RemoveListener(OnReleased);
    }

    private void LateUpdate()
    {
        if (placed || head == null) return;
        // Never move a ring that the user has already picked up.
        if (grab.isSelected) { placed = true; return; }
        var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (!device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) || !tracked)
        {
            trackedTime = 0f;
            return;
        }
        trackedTime += Time.unscaledDeltaTime;
        if (trackedTime < 0.25f) return;
        TryPlaceInFront();
    }

    /// <summary>Returns an unheld ring to the current head pose. Caller checks tracking availability.</summary>
    public bool TryPlaceInFront()
    {
        if (head == null || grab == null || grab.isSelected) return false;
        // XRI completes detach physics after selection exits; do not race that work.
        if (Time.frameCount <= lastReleaseFrame + 1) return false;
        var forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.01f) return false;
        body.useGravity = false;
        body.position = head.position + forward.normalized * forwardDistance - Vector3.up * belowEyes;
        body.rotation = Quaternion.identity;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        placed = true;
        return true;
    }
}
