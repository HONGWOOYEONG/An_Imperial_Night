using UnityEditor.Rendering;
using UnityEngine;

public enum FieldType
{
    Field, // 필드전, 런타임 시간 표기
    FieldTimeOut,
    Boss, // 보스전 , 장막 시간 표기
    BossTimeOut,
    BossClear // 다음 필드 진입 전까지 타이머 멈추기(이때 수주옥 고를듯)
}

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [Header("remain")]
    [SerializeField] private float runRemainingTime = 1800f;
    [SerializeField] private int remainingFlameCount = 3;
    [SerializeField] private float curtainTimeMax = 600f;
    [SerializeField] private float currentCurtainTime = 100f;
    [SerializeField] private FieldType currentField; // 테스트용으로 인스펙터에서 넣을 수 있도록


    private int hRevive = 0;
    private int tRevive = 0;
    public int HRevive => hRevive;
    public int TRevive => tRevive;

    public float RunRemainingTime => runRemainingTime;
    public int RemainingFlameCount => remainingFlameCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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
            // 이후 종료 코드 작성
            Debug.Log("제한시간 종료");
        }

        if(currentField == FieldType.Boss)
        {
            if(currentCurtainTime > 0f)
            {
                currentCurtainTime -= Time.deltaTime;
            }
            else 
            {
                Debug.Log("장막시간 종료");
                currentField = FieldType.BossTimeOut;
                currentCurtainTime = 0f; 
            }
        }
    }

    public void flameRevive(PlayerHealth playerHealth)
    {
        if(remainingFlameCount > 0)
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
    } // 호출할때 this로 보내기

    public float AtkPlus(PlayerType playerType)
    {
        if (playerType == PlayerType.H) return 1 + tRevive * 0.1f;
        else return 1 + hRevive * 0.1f; // revive 횟수에 따라 1.x 증가
    } // 캐릭터 사망시, 다른 플레이어블 캐릭터 공격력 증가

    public void IncreaseCurtainTime(float t)
    {
        
    }
}