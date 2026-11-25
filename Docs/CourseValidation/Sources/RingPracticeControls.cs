using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class RingPracticeControls : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private RingScore target;
    [SerializeField] private float distance = 0.45f;
    [SerializeField] private float belowEyes = 0.35f;

    private Rigidbody body;
    private XRGrabInteractable grab;
    private InputAction returnAction;
    private InputAction newPlayerAction;
    private int lastReleaseFrame = -10;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(OnReleased);
        returnAction = new InputAction("Return", InputActionType.Button,
            "<XRController>{RightHand}/{PrimaryButton}");
        newPlayerAction = new InputAction("New Player", InputActionType.Button,
            "<XRController>{RightHand}/{SecondaryButton}");
    }

    private void OnEnable()
    {
        returnAction.Enable();
        newPlayerAction.Enable();
    }

    private void OnDisable()
    {
        returnAction.Disable();
        newPlayerAction.Disable();
    }

    private void OnDestroy()
    {
        grab.selectExited.RemoveListener(OnReleased);
        returnAction.Dispose();
        newPlayerAction.Dispose();
    }

    private void OnReleased(SelectExitEventArgs args)
        => lastReleaseFrame = Time.frameCount;

    private void Update()
    {
        bool newPlayer = newPlayerAction.WasPressedThisFrame();
        if (!newPlayer && !returnAction.WasPressedThisFrame()) return;

        var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (!device.TryGetFeatureValue(
            UnityEngine.XR.CommonUsages.isTracked, out bool tracked) || !tracked) return;

        TryReturn(newPlayer);
    }

    public bool TryReturn(bool clearScore)
    {
        if (head == null || target == null || grab.isSelected ||
            Time.frameCount <= lastReleaseFrame + 1) return false;

        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.01f) return false;

        // 起動時の待機処理が残っていても、後から輪を動かさない。
        var placement = GetComponent<PlaceRingAtStart>();
        if (placement != null) placement.StopAllCoroutines();

        body.useGravity = false;
        body.position = head.position + forward.normalized * distance
            - Vector3.up * belowEyes;
        body.rotation = Quaternion.identity;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        if (clearScore) target.BeginNewPlayer();
        else target.BeginNextThrow();
        return true;
    }
}

