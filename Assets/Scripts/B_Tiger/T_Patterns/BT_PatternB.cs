using UnityEngine;

public class BT_PatternB : BT_Pattern
{
    private const string JumpToBackwardEvent = "JumpToBackward";
    private const string ThrowGhostEvent = "ThrowGhost";
    private const string FreezeEvent = "Freeze";
    private const string IncreaseGravityEvent = "IncreaseGravity";
    private const string ClearStatusEvent = "ClearStatus";
    private const string WalkToForewardEvent = "WalkToForeward";
    private const string StopWalkEvent = "StopWalk";
    private const string CompleteEvent = "Complete";

    [Header("Pattern B Settings")]
    [SerializeField] private float backWardJumpPower = 15f;
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Transform ghostPoint;
    [SerializeField] private float[] ghostLandingDistances = { 0f, 10f, 20f };
    [SerializeField] private float minGhostFlightTime = 0.55f;
    [SerializeField] private float maxGhostFlightTime = 1.05f;

    private B_TigerController controller;
    private Rigidbody2D rb;
    private float originalGravity;

    public override string PatternId => "PatternB";

    private void Awake()
    {
        controller = GetComponent<B_TigerController>();
    }

    public override void Begin(BT_PatternExecutor patternExecutor, BossPatternData patternData, PlayerContext target)
    {
        base.Begin(patternExecutor, patternData, target);
        EnsureRuntimeReferences();
    }

    public override void OnAnimationEvent(string eventName)
    {
        EnsureRuntimeReferences();

        switch (eventName)
        {
            case JumpToBackwardEvent:
                JumpToBackward();
                break;
            case ThrowGhostEvent:
                ThrowGhost();
                break;
            case FreezeEvent:
                Freeze();
                break;
            case IncreaseGravityEvent:
                IncreaseGravity();
                break;
            case ClearStatusEvent:
                ClearStatus();
                break;
            case WalkToForewardEvent:
                WalkToForeward();
                break;
            case StopWalkEvent:
                controller.Stop();
                break;
            case CompleteEvent:
                Complete();
                break;
        }
    }

    private void EnsureRuntimeReferences()
    {
        if (controller == null)
        {
            controller = GetComponent<B_TigerController>();
        }

        if (rb != null)
        {
            return;
        }

        rb = controller.Rb;
        originalGravity = rb.gravityScale;
    }

    private void JumpToBackward()
    {
        rb.AddForce(new Vector2(-controller.FacingDirection * 1.3f, 0.8f) * backWardJumpPower, ForceMode2D.Impulse);
    }

    private void Freeze()
    {
        rb.gravityScale /= 3;
        rb.linearVelocity /= 6;
    }

    private void IncreaseGravity()
    {
        rb.gravityScale = originalGravity * 2;
    }

    private void ClearStatus()
    {
        rb.gravityScale = originalGravity;
    }

    private void WalkToForeward()
    {
        rb.linearVelocity = new Vector2(controller.FacingDirection * 2.5f, rb.linearVelocity.y);
    }

    private void ThrowGhost()
    {
        for (int i = 0; i < ghostLandingDistances.Length; i++)
        {
            GameObject ghost = Instantiate(ghostPrefab, ghostPoint.position, Quaternion.identity);
            BTP_GhostBall ghostBall = ghost.GetComponent<BTP_GhostBall>();

            float landingX = ghostPoint.position.x + controller.FacingDirection * ghostLandingDistances[i];
            float flightTime = Random.Range(minGhostFlightTime, maxGhostFlightTime);
            ghostBall.ThrowToLandingPoint(landingX, flightTime);
        }
    }
}
