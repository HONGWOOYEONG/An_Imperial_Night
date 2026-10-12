using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FieldType
{
    Field,        // �ʵ���, ��Ÿ�� �ð� ǥ��
    FieldTimeOut,
    FieldClear,   // �� ���� �Ϸ�, �ݶ��̴� ���� ��� ����
    Boss,         // ������, �帷 �ð� ǥ��
    BossTimeOut,
    BossClear     // ���� óġ �Ϸ�, EŰ �Է� ��� ����
}

public class GameSessionManager : MonoBehaviour
{
    private AugmentManager ag;
    private Boss_Health bossHealth;
    private bool bossClearHandled;
    private PlayerHealth hPlayerHealth;
    private PlayerHealth tPlayerHealth;
    private bool gameOverHandled;
    private readonly HashSet<PlayerHealth> pendingRevives = new HashSet<PlayerHealth>();
    [SerializeField, Min(0f)] private float bossDeathDelay = 2f;
    public static GameSessionManager Instance { get; private set; }

    [Header("remain")]
    [SerializeField] private float runRemainingTime = 1800f;
    [SerializeField] private int remainingFlameCount = 3;
    [SerializeField] private float curtainTimeMax = 600f;
    [SerializeField] private float currentCurtainTime;
    [SerializeField] private FieldType currentField;

    private int hRevive = 0;
    private int tRevive = 0;

    public int HRevive => hRevive;
    public int TRevive => tRevive;
    public float RunRemainingTime => runRemainingTime;
    public float CurrentCurtainTime => currentCurtainTime;
    public float CurrentDisplayTime => IsBossField(currentField)
        ? currentCurtainTime
        : runRemainingTime;
    public int RemainingFlameCount => remainingFlameCount;
    public FieldType CurrentField => currentField;

    public event System.Action<FieldType> OnFieldStateChanged;

    private static bool IsBossField(FieldType fieldType)
    {
        return fieldType == FieldType.Boss ||
               fieldType == FieldType.BossTimeOut ||
               fieldType == FieldType.BossClear;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentCurtainTime = curtainTimeMax;
        ag = AugmentManager.Instance;
    }

    private void Start()
    {
        BindBossDeathEvent();
        BindPlayerHealth(); // * 세션이 두 플레이어의 실제 사망 여부를 확인할 수 있도록 참조 연결
    }

    private void BindPlayerHealth()
    {
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (PlayerHealth player in players)
        {
            if (player.PlayerType == PlayerType.H)
                hPlayerHealth = player;
            else if (player.PlayerType == PlayerType.T)
                tPlayerHealth = player;
        }
    }

    private void BindBossDeathEvent()
    {
        if (bossHealth != null) return;

        bossHealth = FindFirstObjectByType<Boss_Health>(FindObjectsInactive.Include);
        if (bossHealth != null)
            bossHealth.OnDead += OnBossDefeated;
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
            bossHealth.OnDead -= OnBossDefeated;

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!gameOverHandled && hPlayerHealth != null && tPlayerHealth != null &&
            hPlayerHealth.IsDead && tPlayerHealth.IsDead &&
            !pendingRevives.Contains(hPlayerHealth) && !pendingRevives.Contains(tPlayerHealth))
        {
            gameOverHandled = true; // * 두 플레이어가 모두 실제 사망했을 때 패배 처리를 한 번만 실행
            MainMenuController.LoadGameOverScene(); // * 메인메뉴 씬으로 이동하면서 GameOver 화면을 요청
            Destroy(gameObject); // * DontDestroyOnLoad 세션 데이터가 메인메뉴까지 남지 않도록 정리
            return;
        }

        if (runRemainingTime > 0)
        {
            runRemainingTime -= Time.deltaTime;
        }
        else if (runRemainingTime < 0)
        {
            runRemainingTime = 0f;
        }

        if (currentField == FieldType.Boss)
        {
            if (currentCurtainTime > 0f)
            {
                currentCurtainTime -= Time.deltaTime;
            }
            else
            {
                currentField = FieldType.BossTimeOut;
                currentCurtainTime = 0f;
                OnFieldStateChanged?.Invoke(currentField);
            }
        }
    }

    public void OnFieldExitPointReached()
    {
        if (currentField != FieldType.FieldClear) return;
        currentField = FieldType.Boss;
        currentCurtainTime = curtainTimeMax;

        Debug.Log($"CameraManager.Instance == null? {CameraManager.Instance == null}");
        CameraManager.Instance.SetBossCamera();

        OnFieldStateChanged?.Invoke(currentField);
    }

    IEnumerator EndBoss()
    {
        yield return new WaitForSeconds(5f);
    }

    public void OnProceedToNextField()
    {
        if (currentField != FieldType.BossClear) return;
        currentField = FieldType.Field;
        CameraManager.Instance.SetFieldCamera();
        OnFieldStateChanged?.Invoke(currentField);
    }


    public void OnBossDefeated()
    {
        if (bossClearHandled) return;
        bossClearHandled = true;

        currentField = FieldType.BossClear;
        OnFieldStateChanged?.Invoke(currentField);

        if (ag == null)
            ag = AugmentManager.Instance;

        if (remainingFlameCount < 1)
        {
            MainMenuController.LoadGameOverScene(); // * 불꽃 없이 보스를 처치하면 승리 조건을 충족하지 못해 패배 화면으로 이동
            Destroy(gameObject);
            return;
        }

        if (ag == null)
        {
            Debug.LogError("GameSessionManager: AugmentManager가 없어 승리 증강 선택을 시작할 수 없습니다.");
            return;
        }

        // * 보스 사망 판정 직후 입력을 끄지 않고 Death 모션을 보여준 다음 증강 선택을 시작합니다.
        StartCoroutine(BeginAugmentAfterBossDeath());
        switch (remainingFlameCount)
        {
            case 3: currentCurtainTime = 600f; break;
            case 2: currentCurtainTime = 420f; break;
            case 1: currentCurtainTime = 360f; break;
        }
    }

    private IEnumerator BeginAugmentAfterBossDeath()
    {
        yield return new WaitForSeconds(bossDeathDelay);

        if (ag == null)
            yield break;

        ag.OnSequenceCompleted -= OnVictoryAugmentCompleted;
        ag.OnSequenceCompleted += OnVictoryAugmentCompleted;
        ag.StartAugmentSequence(remainingFlameCount); // * Death 모션 이후에만 플레이어 입력을 잠그고 증강을 표시합니다.
    }

    private void OnVictoryAugmentCompleted()
    {
        if (ag != null)
            ag.OnSequenceCompleted -= OnVictoryAugmentCompleted;

        MainMenuController.LoadVictoryScene(); // * 마지막 증강 선택이 끝난 뒤에만 승리 화면으로 이동
        Destroy(gameObject);
    }

    public void flameRevive(PlayerHealth playerHealth)
    {
        if (remainingFlameCount > 0)
        {
            remainingFlameCount--;
            if (playerHealth.PlayerType == PlayerType.H) hRevive++;
            else tRevive++;

            // 먼저 실제 사망 상태로 들어가 Death 모션을 보여준 뒤 부활합니다.
            playerHealth.Death();
            if (pendingRevives.Add(playerHealth))
                StartCoroutine(ReviveAfterDeath(playerHealth));
        }
        else
        {
            playerHealth.Death();
        }
    }

    private IEnumerator ReviveAfterDeath(PlayerHealth playerHealth)
    {
        yield return new WaitForSeconds(2f);

        pendingRevives.Remove(playerHealth);
        if (playerHealth != null)
            playerHealth.Revive();
    }

    public float AtkPlus(PlayerType playerType)
    {
        if (playerType == PlayerType.H) return 1 + tRevive * 0.1f;
        else return 1 + hRevive * 0.1f;
    }
}
