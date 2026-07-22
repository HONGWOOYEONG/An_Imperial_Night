using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class T_Defence : MonoBehaviour, IDamageReceiver
{
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    public const float BASE_FPS = 60f;

    [Header("드라이브 게이지")]
    [SerializeField] public float driveGauge = 0f;
    [SerializeField] public float dg_max = 1000f; //드라이브 게이지 최대치
    [SerializeField] float dg_health = 50f; //드라이브 초당 회복량
    [SerializeField] float dg_delay = 3f; //드라이브 회복 시작 지연시간
    private Coroutine regenCoroutine;

    [Header("방어")]
    [SerializeField] float d_driveDecease = 0; //방어 시 감소하는 드라이브 게이지
    [SerializeField] float d_startDelay = 2f; //방어 시작 딜레이
    [SerializeField] float d_endDelay = 2f; //방어 해제 딜레이
    private Coroutine defenceCoroutine;
    

    public bool isDefencing = false; //지금 방어 키를 눌렀나?
    //isHoldingDefence가 true일 때 적이 공격을 하면 방어 성공
    private bool isHoldingDefence = false; //방어를 성공 했나?


    [Header("번아웃")]
    bool isBunOut = false; //번아웃인가?

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        driveGauge = dg_max;
    }
  
    // Update is called once per frame
    void Update()
    {
        //번아웃 true 전환
        if (driveGauge <= 0 && isBunOut == false)
        {
            isBunOut = true;
            ForceStopDefense();//번아웃 시 강제로 방어 헤제
        }
    }

    public void ReceiveAttack(DamageInfo damageInfo)
    {
     
        if (isHoldingDefence)
        {
            Debug.Log("방어 성공");
            driveGauge += damageInfo.driveDamage;
         
            return;
        }

        playerHealth.DamagedFromAtk(damageInfo);
    }

    public void OnDefence(InputValue value) //방어키 입력
    {
        if (value.isPressed)
        {
            isDefencing = true;
            if (!isBunOut && defenceCoroutine == null)
            {
                Debug.Log("방어 시작");
                defenceCoroutine = StartCoroutine(Defence());//defense 코루틴 시작
            }
        }
        else //방어키를 입력을 안하고 있을 때
        {
            isDefencing = false;
            if (defenceCoroutine != null)
            {
                StopCoroutine(defenceCoroutine);
                defenceCoroutine = null;
            }   
            StartCoroutine(EndDefence()); //종료 코루틴 시작
        }
    }
    private void ForceStopDefense() //강제 방어 종료
    {
        if (defenceCoroutine != null)
        {
            StopCoroutine(defenceCoroutine); //현재 진행중인 방어를 종료
            defenceCoroutine = null; //코루틴 변수 비워줌
            StartCoroutine(EndDefence()); //방어 종료 코루틴 시작
        }
    }
    IEnumerator Defence() //방어
    {
        yield return new WaitForSeconds(FrameToSeconds(d_startDelay));//방어 시작 딜레이
        isHoldingDefence = true;
        Debug.Log("방어 중");
        if (playerMovement != null) playerMovement.SetDefending(true); //방어 true알람
    }
    IEnumerator EndDefence() //방어 종료
    {
        yield return new WaitForSeconds(d_endDelay);//방어 해제 딜레이
        if (playerMovement != null) playerMovement.SetDefending(false); //방어 false알림  
        isHoldingDefence = false;
    }

     

    //일정 시간마다 드라이브 게이지 회복 
    IEnumerator RegenDriveGauge()
    {
        yield return new WaitForSeconds(dg_delay); //드라이브 회복 전 지연시간
        while (driveGauge < dg_max) //최대 드라이브 게이지 전까지 회복
        {
            driveGauge += dg_health * Time.deltaTime; //드라이브 게이지 초당 회복
            yield return null; //다음 프레임까지 대기
        }
        if (driveGauge >= dg_max) //드라이브 게이지가 최대 드라이브 게이지보다 크거나 같으면
        {
            isBunOut = false; //isBunOut을 false로 변경
        }
        regenCoroutine = null;
    }

    //드라이브 게이지 감소 함수
    public void DecreaseDriveGauge(float amount)
    {
        driveGauge = (driveGauge - amount) <= 0 ? 0 : (driveGauge - amount); //드라이브게이지 감소
        if (regenCoroutine != null) //실행 중인 코루틴이 있다면 
        {
            StopCoroutine(regenCoroutine); //멈추게 함
        }
        regenCoroutine = StartCoroutine(RegenDriveGauge()); //코루틴 시작
    }

    // 약공이나 강공을 적중 시키면 드라이브게이지 회복 
    public void HealthSomeOfDriveGauge(float amount)
    {
        
        driveGauge = (driveGauge + amount) >= dg_max ? dg_max : (driveGauge + amount);
        if (driveGauge >= dg_max)
        {
            isBunOut = false;
        }
    }

    //특공
    public bool GetIsbunout()
    {
        return isBunOut;
    }

    public float GetCurrentDriveGauge()
    {
        return driveGauge;
    }

    private float FrameToSeconds(float frame)
    {
        return frame / BASE_FPS;
    }
    //드라이브 게이지 최대치 = 1000
    //드라이브 게이지 시작치 = 1000
    //키를 사용하고 드라이브가 회복되는 지연 시간은 3프레임
    //드라이브 게이지 초당 회복량은 50
    //일반 방어를 할때에는 드라이브 게이지가 감소 되지 않는데 방어가 성공이 된다면 드라이브 게이지가 감소

    //방어를 시작 할 때 딜레이가 있고 방어 해제 할 때 딜레이가 있다. 
    //방어 중 이동 속도 배율이 있는데 이동 속도는 PlayerMovement 스크립트에서 지정
   

}
