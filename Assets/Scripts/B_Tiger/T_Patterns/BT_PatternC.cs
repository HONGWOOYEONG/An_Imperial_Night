using UnityEngine;
using System.Collections;

public class BT_PatternC : BT_Pattern
{
    private const string JumpToTopEvent = "JumpToTop";
    private const string FreezeEvent = "Freeze";
    private const string ClearStatusEvent = "ClearStatus";
    private const string RushToTargetEvent = "RushToTarget";

    private Coroutine rushCoroutine;
    private Coroutine patternCoroutine;

    public override string PatternId => "PatternC";

    private B_TigerController controller;
    private Rigidbody2D rb;
    private float originalGravity;
    private Vector2 targetPosition;

    [Header("Pattern C Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float rushSpeed = 20f;
    [SerializeField] private float arrivalDistance = 0.1f;

    public override void Begin(BT_PatternExecutor patternExecutor, BossPatternData patternData, PlayerContext target)
    {
        base.Begin(patternExecutor, patternData, target);
        EnsureRuntimeReferences();

        if (target == null)
        {
            Complete();
            return;
        }

        // 변경: 패턴 시작 시 가장 먼 타겟의 위치를 고정하여 돌진 중에는 유도하지 않습니다.
        targetPosition = target.getPosition();

        // 변경: 위로 점프하기 전에 선택된 타겟 방향을 바라봅니다.
        controller.RotationToTarget(targetPosition - rb.position);

        patternCoroutine = StartCoroutine(PatternCRoutine());
    }

    private void Awake()
    {
        controller = GetComponent<B_TigerController>();
    }

    public override void OnAnimationEvent(string eventName)
    {
        EnsureRuntimeReferences();
        switch (eventName)
        {
            case JumpToTopEvent:
                JumpToTop();
                break;
            case FreezeEvent:
                Freeze();
                break;
            case ClearStatusEvent:
                ClearStatus();
                break;
            case RushToTargetEvent:
                RushToTarget();
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

    private void JumpToTop()
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void Freeze()
    {
        rb.gravityScale /= 3;
        rb.linearVelocity /= 6;
    }

    public override void Stop()
    {
        if (rushCoroutine != null)
        {
            StopCoroutine(rushCoroutine);
            rushCoroutine = null;
        }

    }

    private void ClearStatus()
    {
        rb.gravityScale = originalGravity;
    }

    private void RushToTarget()
    {
        if (rushCoroutine != null)
        {
            return;
        }

        rushCoroutine = StartCoroutine(RushRoutine());
    }

    private IEnumerator RushRoutine()
    {
        rb.linearVelocity = Vector2.zero;

        while (Vector2.Distance(rb.position, targetPosition) > arrivalDistance)
        {
            Vector2 nextPosition = Vector2.MoveTowards(rb.position,targetPosition,rushSpeed * Time.fixedDeltaTime);

            rb.MovePosition(nextPosition);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPosition);
        rb.linearVelocity = Vector2.zero;
        rushCoroutine = null;
        Complete();
    }

    private IEnumerator PatternCRoutine()
    {
        JumpToTop();
        yield return new WaitForSeconds(0.5f);
        Freeze();
        yield return new WaitForSeconds(0.3f);
        ClearStatus();
        RushToTarget();
    }
}
