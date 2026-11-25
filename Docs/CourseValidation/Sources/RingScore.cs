using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Targetに付ける。輪の中心・傾き・速さを毎回調べる。
public class RingScore : MonoBehaviour
{
    [SerializeField] private Rigidbody ring;
    [SerializeField] private float centerTolerance = 0.075f;
    [SerializeField] private float minHeight = 0.10f;
    [SerializeField] private float maxHeight = 0.18f;
    [SerializeField] private float uprightDot = 0.95f;
    [SerializeField] private float maxSpeed = 0.15f;
    [SerializeField] private float maxSpin = 0.5f;
    [SerializeField] private float settleSeconds = 0.5f;

    private XRGrabInteractable grab;
    private bool scoredThisThrow;
    private float settledTime;
    public int Score { get; private set; }

    private void Awake()
    {
        if (ring == null || !ring.TryGetComponent(out grab))
        {
            Debug.LogError("RingにRigidbody付きの輪を登録してください。", this);
            enabled = false;
            return;
        }
    }

    private void FixedUpdate() => CheckScore(Time.fixedDeltaTime);

    public void CheckScore(float seconds)
    {
        if (ring == null || grab == null ||
            grab.isSelected || scoredThisThrow)
        {
            settledTime = 0f;
            return;
        }

        Vector3 local = transform.InverseTransformPoint(ring.position);
        float horizontalDistance = new Vector2(local.x, local.z).magnitude;
        bool inside = horizontalDistance <= centerTolerance &&
            local.y >= minHeight && local.y <= maxHeight;
        bool flat = Mathf.Abs(Vector3.Dot(ring.rotation * Vector3.up, transform.up)) >= uprightDot;
        bool slow = ring.linearVelocity.magnitude <= maxSpeed &&
            ring.angularVelocity.magnitude <= maxSpin;

        settledTime = inside && flat && slow ? settledTime + seconds : 0f;
        if (settledTime < settleSeconds) return;

        scoredThisThrow = true;
        Score++;
        Debug.Log("Score: " + Score, this);
    }

    // 輪を戻せたときだけ、次の投球を得点できるようにする。
    public void BeginNextThrow()
    {
        scoredThisThrow = false;
        settledTime = 0f;
    }

    public void BeginNewPlayer()
    {
        Score = 0;
        BeginNextThrow();
    }
}

