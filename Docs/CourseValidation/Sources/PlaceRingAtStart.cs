using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class PlaceRingAtStart : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private float distance = 0.45f;
    [SerializeField] private float belowEyes = 0.35f;

    private IEnumerator Start()
    {
        if (head == null)
        {
            Debug.LogError("HeadにMain Cameraを入れてください。", this);
            yield break;
        }

        Rigidbody body = GetComponent<Rigidbody>();
        XRGrabInteractable grab = GetComponent<XRGrabInteractable>();
        float trackedSeconds = 0f;

        while (true)
        {
            // 先に掴んだ場合は、その輪を移動させない。
            if (grab.isSelected)
                yield break;

            var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            bool tracked = device.TryGetFeatureValue(
                CommonUsages.isTracked, out bool value) && value;
            trackedSeconds = tracked
                ? trackedSeconds + Time.unscaledDeltaTime : 0f;

            Vector3 forward = Vector3.ProjectOnPlane(
                head.forward, Vector3.up);

            if (trackedSeconds >= 0.25f && forward.sqrMagnitude > 0.01f)
            {
                body.useGravity = false;
                body.position = head.position
                    + forward.normalized * distance
                    - Vector3.up * belowEyes;
                body.rotation = Quaternion.identity;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                yield break;
            }

            yield return null;
        }
    }
}

