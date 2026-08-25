using System.Collections;
using UnityEngine;

public enum FieldType
{
    Field,        // 필드전, 런타임 시간 표기
    FieldTimeOut,
    FieldClear,   // 적 전멸 완료, 콜라이더 도달 대기 상태
    Boss,         // 보스전, 장막 시간 표기
    BossTimeOut,
    BossClear     // 보스 처치 완료, E키 입력 대기 상태
}

public class GameSessionManager : MonoBehaviour
{
    private AugmentManager ag;
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
    public int RemainingFlameCount => remainingFlameCount;
    public FieldType CurrentField => currentField;

    public event System.Action<FieldType> OnFieldStateChanged;

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

    private void Update()
    {
        if (runRemainingTime > 0)
        {
            runRemainingTime -= Time.deltaTime;
        }
        else if (runRemainingTime < 0)
        {
            runRemainingTime = 0f;
            Debug.Log("제한시간 종료");
        }

        if (currentField == FieldType.Boss)
        {
            if (currentCurtainTime > 0f)
            {
                currentCurtainTime -= Time.deltaTime;
            }
            else
            {
                Debug.Log("장막시간 종료");
                currentField = FieldType.BossTimeOut;
                currentCurtainTime = 0f;
                OnFieldStateChanged?.Invoke(currentField);
            }
        }
    }

    public void OnFieldExitPointReached()
    {
        if (currentField != FieldType.Field && currentField != FieldType.FieldClear) return;
        currentField = FieldType.Boss;
        currentCurtainTime = curtainTimeMax;
        Debug.Log("보스전 진입");

        Debug.Log($"CameraManager.Instance == null? {CameraManager.Instance == null}");
        CameraManager.Instance.SetBossCamera();

        OnFieldStateChanged?.Invoke(currentField);

        StartCoroutine(EndBoss());
        currentField = FieldType.BossClear;
        OnFieldStateChanged?.Invoke(currentField);
        clearBoss();
    }

    IEnumerator EndBoss()
    {
        yield return new WaitForSeconds(5f);
    }

    public void OnProceedToNextField()
    {
        if (currentField != FieldType.BossClear) return;
        currentField = FieldType.Field;
        Debug.Log("다음 필드 진입");
        CameraManager.Instance.SetFieldCamera();
        OnFieldStateChanged?.Invoke(currentField);
    }


    private void clearBoss()
    {
        ag?.StartAugmentSequence(remainingFlameCount);
        switch (remainingFlameCount)
        {
            case 3: currentCurtainTime = 600f; break;
            case 2: currentCurtainTime = 420f; break;
            case 1: currentCurtainTime = 360f; break;
        }
    }

    public void flameRevive(PlayerHealth playerHealth)
    {
        if (remainingFlameCount > 0)
        {
            playerHealth.Revive();
            remainingFlameCount--;
            if (playerHealth.PlayerType == PlayerType.H) hRevive++;
            else tRevive++;
        }
        else
        {
            playerHealth.Death();
        }
    }

    public float AtkPlus(PlayerType playerType)
    {
        if (playerType == PlayerType.H) return 1 + tRevive * 0.1f;
        else return 1 + hRevive * 0.1f;
    }
}