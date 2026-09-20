using System.Collections;
using UnityEngine;

public class BT_PatternA : BT_Pattern
{
    private enum PatternACase
    {
        Case1 = 0,
        Case2 = 1
    }

    [Header("Pattern A Settings")]
    [SerializeField] private float n1 = 5f;
    [SerializeField] private float inhaleTime = 1f;
    [SerializeField] private float recoveryTime = 0.5f;

    [Header("Case 1 - Ghost Lancer")]
    [SerializeField] private GameObject ghostLancerPrefab;
    [SerializeField] private Transform ghostSpawnPoint;
    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private GameObject daggerHitboxPrefab;
    [SerializeField] private float n2 = 10f;
    [SerializeField] private float trackingSpeed = 6f;
    [SerializeField] private float spearSpeed = 15f;
    [SerializeField] private float spearDelay = 0.3f;
    [SerializeField] private float turnTime = 0.6f;
    [SerializeField] private float daggerDelay = 0.3f;
    [SerializeField] private float daggerRushSpeed = 18f;

    [Header("Case 2 - Smoke Explosion")]
    [SerializeField] private GameObject smokeCloudPrefab;
    [SerializeField] private Transform smokeSpawnPoint;
    [SerializeField] private GameObject smokeExplosionPrefab;
    [SerializeField] private float smokeSpeed = 8f;
    [SerializeField] private float backwardForce = 5f;
    [SerializeField] private float backwardTime = 0.35f;
    [SerializeField] private float ignitionDelay = 0.6f;
    [SerializeField] private float explosionTime = 0.3f;

    private B_TigerController controller;
    private Coroutine patternCoroutine;
    private GameObject spawnedGhostLancer;
    private GameObject spawnedSmokeCloud;
    private GameObject spawnedAttackObject;
    private Sprite previewSprite;
    private PatternACase selectedCase;

    public override string PatternId => "PatternA";

    private void Awake()
    {
        controller = GetComponent<B_TigerController>();
    }

    public override void Begin(
        BT_PatternExecutor patternExecutor,
        BossPatternData patternData,
        PlayerContext target)
    {
        base.Begin(patternExecutor, patternData, target);

        EnsureController();

        PlayerContext nearestTarget = controller.GetNearestTarget();
        if (nearestTarget == null)
        {
            Complete();
            return;
        }

        float nearestDistance = Vector2.Distance(
            transform.position,
            controller.GetTargetPosition(nearestTarget));

        // 가까운 캐릭터가 n1 안이면 Case 1/2 중 하나를 랜덤으로 선택합니다.
        // n1 밖이면 기획대로 Case 1을 확정합니다.
        if (nearestDistance <= n1)
        {
            selectedCase = Random.Range(0, 2) == 0
                ? PatternACase.Case1
                : PatternACase.Case2;
        }
        else
        {
            selectedCase = PatternACase.Case1;
        }

        controller.Animator.SetInteger("ACase", (int)selectedCase);
        Debug.Log($"Pattern A Coroutine Start - {selectedCase}", this);
        patternCoroutine = StartCoroutine(RunPattern());
    }

    public override void OnAnimationEvent(string eventName)
    {
        // 현재는 Pattern C처럼 코루틴으로 전체 흐름을 시연합니다.
        // 애니메이션이 제작되면 필요한 동작을 이 switch에 하나씩 연결하면 됩니다.
        if (eventName == "Complete")
        {
            FinishPattern();
        }
    }

    private IEnumerator RunPattern()
    {
        // 담배를 들이마시는 임시 대기 시간입니다.
        yield return new WaitForSeconds(inhaleTime);

        if (selectedCase == PatternACase.Case1)
        {
            yield return RunCase1();
        }
        else
        {
            yield return RunCase2();
        }

        yield return new WaitForSeconds(recoveryTime);

        patternCoroutine = null;
        CleanupSpawnedObjects();
        Complete();
    }

    private IEnumerator RunCase1()
    {
        PlayerContext target = controller.GetFarthestTarget();
        if (target == null)
        {
            yield break;
        }

        Vector2 spawnPosition = ghostSpawnPoint != null
            ? ghostSpawnPoint.position
            : transform.position;

        // 프리팹이 아직 없으면 청록색 임시 도형을 만들어 움직임만 확인합니다.
        spawnedGhostLancer = CreatePatternObject(
            ghostLancerPrefab,
            spawnPosition,
            "GhostLancer_Preview",
            new Color(0.2f, 0.9f, 1f),
            new Vector2(1.2f, 2.2f));

        // 패턴 시작 전에 발생했던 위치변환 신호는 버립니다.
        controller.ConsumeTargetTransPosition(target);

        Vector2 lockedPosition = controller.GetTargetPosition(target);

        // 타겟의 현재 X 좌표를 계속 갱신하며 n2 거리까지 접근합니다.
        while (Mathf.Abs(spawnedGhostLancer.transform.position.x - lockedPosition.x) > n2)
        {
            // n2 밖에서 위치변환이 발생하면 가장 먼 캐릭터를 다시 선택하고
            // 그 순간의 위치를 고정한 뒤 바로 1타로 넘어갑니다.
            if (controller.ConsumeTargetTransPosition(target))
            {
                target = controller.GetFarthestTarget();
                if (target != null)
                {
                    lockedPosition = controller.GetTargetPosition(target);
                }
                break;
            }

            lockedPosition = controller.GetTargetPosition(target);

            float nextX = Mathf.MoveTowards(
                spawnedGhostLancer.transform.position.x,
                lockedPosition.x,
                trackingSpeed * Time.deltaTime);

            spawnedGhostLancer.transform.position = new Vector3(
                nextX,
                spawnedGhostLancer.transform.position.y,
                spawnedGhostLancer.transform.position.z);

            yield return null;
        }

        // n2 안에 들어온 시점의 X 좌표를 최종 공격 위치로 고정합니다.
        lockedPosition = new Vector2(
            lockedPosition.x,
            spawnedGhostLancer.transform.position.y);

        yield return new WaitForSeconds(spearDelay);

        // 창 프리팹 자체에 히트박스를 넣으면 생성되는 순간 판정이 켜집니다.
        spawnedAttackObject = CreatePatternObject(
            spearPrefab,
            spawnedGhostLancer.transform.position,
            "Spear_Preview",
            Color.white,
            new Vector2(1.5f, 0.2f));

        yield return MoveObject(spawnedAttackObject.transform, lockedPosition, spearSpeed);
        Destroy(spawnedAttackObject);
        spawnedAttackObject = null;

        // 실제 회전은 추후 애니메이션으로 교체할 부분입니다.
        yield return new WaitForSeconds(turnTime);

        // 2타는 귀신창병을 기준으로 가장 가까운 캐릭터를 선택합니다.
        target = controller.GetNearestTarget(spawnedGhostLancer.transform.position);
        if (target == null)
        {
            yield break;
        }

        yield return new WaitForSeconds(daggerDelay);

        // 2타 발생 직전에 타겟 위치를 가져온 뒤 더 이상 갱신하지 않습니다.
        lockedPosition = controller.GetTargetPosition(target);
        lockedPosition.y = spawnedGhostLancer.transform.position.y;

        // 단검 공격 프리팹을 귀신창병의 자식으로 생성하여 돌진을 따라가게 합니다.
        spawnedAttackObject = CreatePatternObject(
            daggerHitboxPrefab,
            spawnedGhostLancer.transform.position,
            "Dagger_Preview",
            Color.red,
            new Vector2(0.8f, 0.3f));
        spawnedAttackObject.transform.SetParent(spawnedGhostLancer.transform, true);

        yield return MoveObject(
            spawnedGhostLancer.transform,
            lockedPosition,
            daggerRushSpeed);

        if (spawnedAttackObject != null)
        {
            Destroy(spawnedAttackObject);
            spawnedAttackObject = null;
        }
    }

    private IEnumerator RunCase2()
    {
        PlayerContext target = controller.GetNearestTarget();
        if (target == null)
        {
            yield break;
        }

        Vector2 spawnPosition = smokeSpawnPoint != null
            ? smokeSpawnPoint.position
            : transform.position;

        Vector2 lockedPosition = controller.GetTargetPosition(target);

        // 프리팹이 아직 없으면 회색 임시 도형으로 연기 이동을 확인합니다.
        spawnedSmokeCloud = CreatePatternObject(
            smokeCloudPrefab,
            spawnPosition,
            "SmokeCloud_Preview",
            Color.gray,
            new Vector2(2.5f, 2.5f));

        // 연기를 내뱉는 반작용으로 보스가 바라보는 방향의 반대로 밀려납니다.
        controller.Rb.linearVelocity = new Vector2(0f, controller.Rb.linearVelocity.y);
        controller.Rb.AddForce(
            Vector2.left * controller.FacingDirection * backwardForce,
            ForceMode2D.Impulse);

        float backwardTimer = 0f;

        // 연기 이동이 먼저 끝나더라도 반작용 시간은 backwardTime만큼 유지합니다.
        while (Vector2.Distance(spawnedSmokeCloud.transform.position, lockedPosition) > 0.05f ||
               backwardTimer < backwardTime)
        {
            if (Vector2.Distance(spawnedSmokeCloud.transform.position, lockedPosition) > 0.05f)
            {
                spawnedSmokeCloud.transform.position = Vector2.MoveTowards(
                    spawnedSmokeCloud.transform.position,
                    lockedPosition,
                    smokeSpeed * Time.deltaTime);
            }

            backwardTimer += Time.deltaTime;
            yield return null;
        }

        StopBackwardMovement();
        yield return new WaitForSeconds(ignitionDelay);

        // 폭발 프리팹 자체에 히트박스를 넣으면 생성과 동시에 판정이 시작됩니다.
        spawnedAttackObject = CreatePatternObject(
            smokeExplosionPrefab,
            spawnedSmokeCloud.transform.position,
            "SmokeExplosion_Preview",
            new Color(1f, 0.55f, 0.1f),
            new Vector2(3f, 3f));

        Destroy(spawnedSmokeCloud);
        spawnedSmokeCloud = null;

        yield return new WaitForSeconds(explosionTime);

        if (spawnedAttackObject != null)
        {
            Destroy(spawnedAttackObject);
            spawnedAttackObject = null;
        }
    }

    private IEnumerator MoveObject(Transform movingObject, Vector2 targetPosition, float speed)
    {
        while (movingObject != null &&
               Vector2.Distance(movingObject.position, targetPosition) > 0.05f)
        {
            movingObject.position = Vector2.MoveTowards(
                movingObject.position,
                targetPosition,
                speed * Time.deltaTime);

            yield return null;
        }
    }

    private GameObject CreatePatternObject(
        GameObject prefab,
        Vector2 position,
        string previewName,
        Color previewColor,
        Vector2 previewScale)
    {
        if (prefab != null)
        {
            return Instantiate(prefab, position, Quaternion.identity);
        }

        // 애니메이션/프리팹 제작 전 시연을 위한 단순 SpriteRenderer입니다.
        GameObject previewObject = new GameObject(previewName);
        previewObject.transform.position = position;
        previewObject.transform.localScale = previewScale;

        SpriteRenderer spriteRenderer = previewObject.AddComponent<SpriteRenderer>();
        if (previewSprite == null)
        {
            previewSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
        }

        spriteRenderer.sprite = previewSprite;
        spriteRenderer.color = previewColor;
        spriteRenderer.sortingOrder = 10;
        return previewObject;
    }

    private void StopBackwardMovement()
    {
        if (controller != null && controller.Rb != null)
        {
            controller.Rb.linearVelocity = new Vector2(0f, controller.Rb.linearVelocity.y);
        }
    }

    private void FinishPattern()
    {
        if (patternCoroutine != null)
        {
            StopCoroutine(patternCoroutine);
            patternCoroutine = null;
        }

        CleanupSpawnedObjects();
        Complete();
    }

    public override void Stop()
    {
        if (patternCoroutine != null)
        {
            StopCoroutine(patternCoroutine);
            patternCoroutine = null;
        }

        StopBackwardMovement();
        CleanupSpawnedObjects();
    }

    private void CleanupSpawnedObjects()
    {
        if (spawnedAttackObject != null) Destroy(spawnedAttackObject);
        if (spawnedGhostLancer != null) Destroy(spawnedGhostLancer);
        if (spawnedSmokeCloud != null) Destroy(spawnedSmokeCloud);

        spawnedAttackObject = null;
        spawnedGhostLancer = null;
        spawnedSmokeCloud = null;
    }

    private void EnsureController()
    {
        if (controller == null)
        {
            controller = GetComponent<B_TigerController>();
        }
    }

    private void OnDestroy()
    {
        if (previewSprite != null)
        {
            Destroy(previewSprite);
        }
    }
}
