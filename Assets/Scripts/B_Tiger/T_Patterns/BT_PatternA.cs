using System.Collections;
using UnityEngine;

public class BT_PatternA : BT_Pattern
{
    private enum PatternACase
    {
        Case1 = 0,
        Case2 = 1
    }

    private const string StartCase1Event = "StartCase1";
    private const string EnableSpearHitboxEvent = "EnableSpearHitbox";
    private const string DisableSpearHitboxEvent = "DisableSpearHitbox";
    private const string SelectSecondTargetEvent = "SelectSecondTarget";
    private const string LockSecondTargetEvent = "LockSecondTarget";
    private const string StartDaggerRushEvent = "StartDaggerRush";
    private const string EnableDaggerHitboxEvent = "EnableDaggerHitbox";
    private const string DisableDaggerHitboxEvent = "DisableDaggerHitbox";
    private const string StartCase2Event = "StartCase2";
    private const string StopBackwardEvent = "StopBackward";
    private const string ExplodeSmokeEvent = "ExplodeSmoke";
    private const string DisableExplosionHitboxEvent = "DisableExplosionHitbox";
    private const string CompleteEvent = "Complete";

    [Header("Pattern A Selection")]
    [SerializeField] private float n1 = 5f;
    [SerializeField] private bool useAnimationEvents;

    [Header("Code-driven Timing")]
    [SerializeField] private float spearHitboxDuration = 0.3f;
    [SerializeField] private float daggerHitboxDuration = 0.35f;
    [SerializeField] private float smokeExplosionDelay = 0.3f;
    [SerializeField] private float explosionHitboxDuration = 0.4f;

    [Header("Case 1 - Ghost Lancer")]
    [SerializeField] private Transform ghostLancer;
    [SerializeField] private Transform ghostSpawnPoint;
    [SerializeField] private Animator ghostAnimator;
    [SerializeField] private Transform attackTargetPoint;
    [SerializeField] private GameObject spearHitbox;
    [SerializeField] private GameObject daggerHitbox;
    [SerializeField] private float n2 = 10f;
    [SerializeField] private float ghostTrackingSpeed = 6f;
    [SerializeField] private float daggerRushSpeed = 18f;
    [SerializeField] private float rushArrivalDistance = 0.1f;
    [SerializeField] private string ghostMovingBool = "IsMoving";
    [SerializeField] private string spearAttackTrigger = "SpearAttack";
    [SerializeField] private string daggerRushTrigger = "DaggerRush";

    [Header("Case 2 - Smoke Explosion")]
    [SerializeField] private Transform smokeCloud;
    [SerializeField] private Transform smokeSpawnPoint;
    [SerializeField] private Animator smokeAnimator;
    [SerializeField] private GameObject explosionHitbox;
    [SerializeField] private float smokeMoveSpeed = 8f;
    [SerializeField] private float backwardForce = 5f;
    [SerializeField] private string smokeMovingBool = "IsMoving";
    [SerializeField] private string smokeExplosionTrigger = "Explosion";

    private B_TigerController controller;

    private PatternACase selectedCase;
    private PlayerContext currentTarget;
    private Vector2 lockedTargetPosition;
    private Coroutine trackingCoroutine;
    private Coroutine daggerRushCoroutine;
    private Coroutine smokeMoveCoroutine;
    private Coroutine sequenceCoroutine;
    private bool isRunning;

    public Vector2 LockedTargetPosition => lockedTargetPosition;

    public override string PatternId => "PatternA";

    private void Awake()
    {
        controller = GetComponent<B_TigerController>();

        DisableAllHitboxes();
        HidePatternObjects();
    }

    public override void Begin(BT_PatternExecutor patternExecutor, BossPatternData patternData, PlayerContext target)
    {
        base.Begin(patternExecutor, patternData, target);
        StopCoroutines();
        DisableAllHitboxes();
        EnsureRuntimeReferences();

        PlayerContext nearestTarget = controller.GetNearestTarget();
        if (nearestTarget == null)
        {
            Complete();
            return;
        }

        float nearestDistance = Vector2.Distance(transform.position, controller.GetTargetPosition(nearestTarget));

        if (nearestDistance <= n1)
        {
            selectedCase = Random.Range(0, 2) == 0 ? PatternACase.Case1 : PatternACase.Case2;
        }
        else
        {
            selectedCase = PatternACase.Case1;
        }

        currentTarget = selectedCase == PatternACase.Case1 ? controller.GetFarthestTarget() : nearestTarget;

        controller.Animator.SetInteger("ACase", (int)selectedCase);

        if (currentTarget != null)
        {
            controller.RotationToTarget(controller.GetTargetPosition(currentTarget) - (Vector2)transform.position);
        }

        isRunning = true;

        if (!useAnimationEvents)
        {
            sequenceCoroutine = StartCoroutine(RunPatternSequence());
        }
    }

    // Pattern A 클립에 이벤트가 없을 때에도 Transform 패턴이 끝까지 진행되게 합니다.
    private IEnumerator RunPatternSequence()
    {
        if (selectedCase == PatternACase.Case1)
        {
            StartCase1();
            yield return new WaitUntil(() => !isRunning || trackingCoroutine == null);

            if (!isRunning)
            {
                yield break;
            }

            SetHitbox(spearHitbox, true);
            yield return new WaitForSeconds(spearHitboxDuration);
            SetHitbox(spearHitbox, false);

            SelectSecondTarget();
            LockSecondTarget();
            StartDaggerRush();
            SetHitbox(daggerHitbox, true);
            yield return new WaitUntil(() => !isRunning || daggerRushCoroutine == null);
            yield return new WaitForSeconds(daggerHitboxDuration);
            SetHitbox(daggerHitbox, false);
        }
        else
        {
            StartCase2();
            yield return new WaitUntil(() => !isRunning || smokeMoveCoroutine == null);

            if (!isRunning)
            {
                yield break;
            }

            StopBackward();
            yield return new WaitForSeconds(smokeExplosionDelay);
            ExplodeSmoke();
            yield return new WaitForSeconds(explosionHitboxDuration);
            SetHitbox(explosionHitbox, false);
        }

        FinishPattern();
    }

    public override void OnAnimationEvent(string eventName)
    {
        if (!isRunning)
        {
            return;
        }

        switch (eventName)
        {
            case StartCase1Event:
                StartCase1();
                break;
            case EnableSpearHitboxEvent:
                SetHitbox(spearHitbox, true);
                break;
            case DisableSpearHitboxEvent:
                SetHitbox(spearHitbox, false);
                break;
            case SelectSecondTargetEvent:
                SelectSecondTarget();
                break;
            case LockSecondTargetEvent:
                LockSecondTarget();
                break;
            case StartDaggerRushEvent:
                StartDaggerRush();
                break;
            case EnableDaggerHitboxEvent:
                SetHitbox(daggerHitbox, true);
                break;
            case DisableDaggerHitboxEvent:
                SetHitbox(daggerHitbox, false);
                break;
            case StartCase2Event:
                StartCase2();
                break;
            case StopBackwardEvent:
                StopBackward();
                break;
            case ExplodeSmokeEvent:
                ExplodeSmoke();
                break;
            case DisableExplosionHitboxEvent:
                SetHitbox(explosionHitbox, false);
                break;
            case CompleteEvent:
                FinishPattern();
                break;
        }
    }

    private void StartCase1()
    {
        if (selectedCase != PatternACase.Case1 || ghostLancer == null)
        {
            if (selectedCase == PatternACase.Case1)
            {
                FinishPattern();
            }
            return;
        }

        currentTarget = controller.GetFarthestTarget();
        if (currentTarget == null)
        {
            FinishPattern();
            return;
        }

        if (ghostSpawnPoint != null)
        {
            ghostLancer.position = ghostSpawnPoint.position;
        }

        ghostLancer.gameObject.SetActive(true);

        controller.ConsumeTargetTransPosition(currentTarget);
        trackingCoroutine = StartCoroutine(TrackFirstTarget());
    }

    private IEnumerator TrackFirstTarget()
    {
        SetAnimatorBool(ghostAnimator, ghostMovingBool, true);

        while (isRunning && currentTarget != null)
        {
            Vector2 targetPosition = controller.GetTargetPosition(currentTarget);
            float xDistance = Mathf.Abs(ghostLancer.position.x - targetPosition.x);

            if (xDistance <= n2)
            {
                LockTargetPosition(targetPosition.x, ghostLancer.position.y);
                break;
            }

            if (controller.ConsumeTargetTransPosition(currentTarget))
            {
                currentTarget = controller.GetFarthestTarget();
                if (currentTarget != null)
                {
                    LockTargetPosition(
                        controller.GetTargetPosition(currentTarget).x,
                        ghostLancer.position.y);
                }
                break;
            }

            float nextX = Mathf.MoveTowards(
                ghostLancer.position.x,
                targetPosition.x,
                ghostTrackingSpeed * Time.deltaTime);

            ghostLancer.position = new Vector3(
                nextX,
                ghostLancer.position.y,
                ghostLancer.position.z);

            FaceTransformToX(ghostLancer, targetPosition.x);
            yield return null;
        }

        trackingCoroutine = null;
        SetAnimatorBool(ghostAnimator, ghostMovingBool, false);

        if (!isRunning || currentTarget == null)
        {
            yield break;
        }

        FaceTransformToX(ghostLancer, lockedTargetPosition.x);
        SetAnimatorTrigger(ghostAnimator, spearAttackTrigger);
    }

    private void SelectSecondTarget()
    {
        if (selectedCase != PatternACase.Case1 || ghostLancer == null)
        {
            return;
        }

        currentTarget = controller.GetNearestTarget(ghostLancer.position);
        if (currentTarget != null)
        {
            FaceTransformToX(
                ghostLancer,
                controller.GetTargetPosition(currentTarget).x);
        }
    }

    private void LockSecondTarget()
    {
        if (currentTarget == null || ghostLancer == null)
        {
            return;
        }

        LockTargetPosition(
            controller.GetTargetPosition(currentTarget).x,
            ghostLancer.position.y);
        FaceTransformToX(ghostLancer, lockedTargetPosition.x);
    }

    private void StartDaggerRush()
    {
        if (selectedCase != PatternACase.Case1 || ghostLancer == null)
        {
            return;
        }

        SetAnimatorTrigger(ghostAnimator, daggerRushTrigger);

        if (daggerRushCoroutine != null)
        {
            StopCoroutine(daggerRushCoroutine);
        }

        daggerRushCoroutine = StartCoroutine(DaggerRushRoutine());
    }

    private IEnumerator DaggerRushRoutine()
    {
        while (isRunning &&
               Mathf.Abs(ghostLancer.position.x - lockedTargetPosition.x) > rushArrivalDistance)
        {
            float nextX = Mathf.MoveTowards(ghostLancer.position.x,lockedTargetPosition.x,daggerRushSpeed * Time.deltaTime);
            ghostLancer.position = new Vector3(nextX,ghostLancer.position.y,ghostLancer.position.z);
            yield return null;
        }

        daggerRushCoroutine = null;
    }

    private void StartCase2()
    {
        if (selectedCase != PatternACase.Case2 || smokeCloud == null)
        {
            if (selectedCase == PatternACase.Case2)
            {
                FinishPattern();
            }
            return;
        }

        if (currentTarget == null)
        {
            currentTarget = controller.GetNearestTarget();
        }

        if (currentTarget == null)
        {
            FinishPattern();
            return;
        }

        lockedTargetPosition = controller.GetTargetPosition(currentTarget);

        if (smokeSpawnPoint != null)
        {
            smokeCloud.position = smokeSpawnPoint.position;
        }

        smokeCloud.gameObject.SetActive(true);
        SetAnimatorBool(smokeAnimator, smokeMovingBool, true);

        if (smokeMoveCoroutine != null)
        {
            StopCoroutine(smokeMoveCoroutine);
        }
        smokeMoveCoroutine = StartCoroutine(MoveSmokeRoutine());

        if (controller.Rb != null)
        {
            controller.Rb.linearVelocity = new Vector2(0f, controller.Rb.linearVelocity.y);
            controller.Rb.AddForce(Vector2.left * controller.FacingDirection * backwardForce,ForceMode2D.Impulse);
        }
    }

    private IEnumerator MoveSmokeRoutine()
    {
        while (isRunning &&
               Vector2.Distance(smokeCloud.position, lockedTargetPosition) > 0.05f)
        {
            smokeCloud.position = Vector2.MoveTowards(smokeCloud.position,lockedTargetPosition,smokeMoveSpeed * Time.deltaTime);

            yield return null;
        }

        smokeMoveCoroutine = null;
        SetAnimatorBool(smokeAnimator, smokeMovingBool, false);
    }

    private void StopBackward()
    {
        if (controller != null && controller.Rb != null)
        {
            controller.Rb.linearVelocity = new Vector2(0f, controller.Rb.linearVelocity.y);
        }
    }

    private void ExplodeSmoke()
    {
        if (selectedCase != PatternACase.Case2 || smokeCloud == null)
        {
            return;
        }

        SetAnimatorTrigger(smokeAnimator, smokeExplosionTrigger);
        SetHitbox(explosionHitbox, true);
    }

    private void LockTargetPosition(float x, float y)
    {
        lockedTargetPosition = new Vector2(x, y);

        if (attackTargetPoint != null)
        {
            attackTargetPoint.position = lockedTargetPosition;
        }
    }

    private static void FaceTransformToX(Transform targetTransform, float targetX)
    {
        float direction = Mathf.Sign(targetX - targetTransform.position.x);
        if (direction == 0f)
        {
            return;
        }

        Vector3 scale = targetTransform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        targetTransform.localScale = scale;
    }

    private static void SetHitbox(GameObject hitbox, bool active)
    {
        if (hitbox != null)
        {
            hitbox.SetActive(active);
        }
    }

    private static void SetAnimatorBool(Animator targetAnimator, string parameterName, bool value)
    {
        if (targetAnimator != null && !string.IsNullOrEmpty(parameterName))
        {
            targetAnimator.SetBool(parameterName, value);
        }
    }

    private static void SetAnimatorTrigger(Animator targetAnimator, string parameterName)
    {
        if (targetAnimator != null && !string.IsNullOrEmpty(parameterName))
        {
            targetAnimator.SetTrigger(parameterName);
        }
    }

    private void DisableAllHitboxes()
    {
        SetHitbox(spearHitbox, false);
        SetHitbox(daggerHitbox, false);
        SetHitbox(explosionHitbox, false);
    }

    private void EnsureRuntimeReferences()
    {
        if (controller == null) controller = GetComponent<B_TigerController>();
    }

    private void FinishPattern()
    {
        if (!isRunning)
        {
            return;
        }

        isRunning = false;
        StopCoroutines();
        StopBackward();
        DisableAllHitboxes();
        HidePatternObjects();
        Complete();
    }

    private void StopCoroutines()
    {
        if (trackingCoroutine != null) StopCoroutine(trackingCoroutine);
        if (daggerRushCoroutine != null) StopCoroutine(daggerRushCoroutine);
        if (smokeMoveCoroutine != null) StopCoroutine(smokeMoveCoroutine);
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);

        trackingCoroutine = null;
        daggerRushCoroutine = null;
        smokeMoveCoroutine = null;
        sequenceCoroutine = null;
    }

    public override void Stop()
    {
        isRunning = false;
        StopCoroutines();
        StopBackward();
        DisableAllHitboxes();

        SetAnimatorBool(ghostAnimator, ghostMovingBool, false);
        SetAnimatorBool(smokeAnimator, smokeMovingBool, false);
        HidePatternObjects();
    }

    private void HidePatternObjects()
    {
        if (ghostLancer != null)
        {
            ghostLancer.gameObject.SetActive(false);
        }

        if (smokeCloud != null)
        {
            smokeCloud.gameObject.SetActive(false);
        }
    }
}
