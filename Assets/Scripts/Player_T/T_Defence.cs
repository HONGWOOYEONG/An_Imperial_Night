using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class T_Defence : MonoBehaviour, IDamageReceiver
{
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private T_DriveGauge t_DriveGauge;
    public const float BASE_FPS = 60f;


    [Header("방어")]
    [SerializeField] float d_driveDecease = 0; //방어 시 감소하는 드라이브 게이지
    [SerializeField] float d_startDelay = 2f; //방어 시작 딜레이
    [SerializeField] float d_endDelay = 2f; //방어 해제 딜레이
    private Coroutine defenceCoroutine = null;
    

    public bool isDefencing = false; //지금 방어 키를 눌렀나?
    //isHoldingDefence가 true일 때 적이 공격을 하면 방어 성공
    private bool isHoldingDefence = false; //방어를 성공 했나?


    

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        t_DriveGauge = GetComponent<T_DriveGauge>();
    }
  
    // Update is called once per frame
    void Update()
    {
       
    }

    //--방어키 입력 함수--
    public void OnDefence(InputValue value) 
    {
        if (value.isPressed)
        {
            isDefencing = true;
            if (!t_DriveGauge.isBunOut && defenceCoroutine == null)
            {
                defenceCoroutine = StartCoroutine(Defence());//defense 코루틴 시작
            }
        }
        else //방어키를 입력을 안하고 있을 때
        {  
            if (defenceCoroutine != null)
            {
                StopCoroutine(defenceCoroutine);
                defenceCoroutine = null;
            }
            isDefencing = false;
            isHoldingDefence = false;

            playerMovement.SetDefending(false);
        }
    }
    //--방어 시작--
    IEnumerator Defence()
    {
        Debug.Log("방어 시작");
        yield return new WaitForSeconds(FrameToSeconds(d_startDelay));//방어 시작 딜레이
        isHoldingDefence = true;
        Debug.Log("방어 중");
        if (playerMovement != null) playerMovement.SetDefending(true); //방어 true알람
        yield return new WaitForSeconds(d_endDelay);//방어 해제 딜레이
    }

    //--적에게서 받아온 damageInfo 정보를 넘겨주는 함수--
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (isHoldingDefence)
        {
            GuardSuccess(damageInfo);
        }
        else
        {
            GuardFail(damageInfo);
        }
    }
    //--방어 성공 함수--
    public void GuardSuccess(DamageInfo damageInfo)
    {
            Debug.Log("방어 성공");
            t_DriveGauge.driveGauge += damageInfo.driveDamage;
    }
    //--방어 실패 함수--
    public void GuardFail(DamageInfo damageInfo)
    {
            Debug.Log("방어 실패");
            playerHealth.DamagedFromAtk(damageInfo);
    }


    //--드라이브 게이지를 다 사용했을 경우 강제 방어 종료 함수--
    public void ForceStopDefense() 
    {
        Debug.Log("방어 강제 종료");
        if (defenceCoroutine != null)
        {
            StopCoroutine(defenceCoroutine); //현재 진행중인 방어를 종료
            defenceCoroutine = null; //코루틴 변수 비워줌
        }
    }


    private float FrameToSeconds(float frame)
    {
        return frame / BASE_FPS;
    }

   
}
