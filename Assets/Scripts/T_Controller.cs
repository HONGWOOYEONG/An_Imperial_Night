using System.Collections;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;

public class T_Controller : MonoBehaviour
{
    private PlayerMovement movement;

    [Header("드라이브 게이지")]
    [SerializeField]float driveGauge = 0f;
    [SerializeField] float dg_max = 1000f; //드라이브 게이지 최대치
    [SerializeField] float dg_health = 50f; //드라이브 초당 회복량
    [SerializeField] float dg_delay = 3f; //드라이브 회복 시작 지연시간
    private Coroutine regenCoroutine;

    [Header("방어")]
    [SerializeField] float d_driveDecease = 0; //방어 시 감소하는 드라이브 게이지
    [SerializeField] float d_startDelay = 2f; //방어 시작 딜레이
    [SerializeField] float d_endDelay = 2f;
    private Coroutine defenseCoroutine;

    //적이 공격을 하고 플레이어가 방어 중이라면 isdefense를 true로 변경
    public bool isdefense = false; //방어를 성공 했나?


    [Header("약공")]
    [SerializeField] float w_attackrange = 3f;//약공 사거리
    [SerializeField] float w_attacktime = 2f;//약공 발사 간격
    private float w_timer = 0;
    //약공 타겟팅 탐색 각도
    //약공 타겟 유지 시간
    [SerializeField] float w_stayCount = 0.5f;//약공 경직 시간


    [Header("강공")]
    [SerializeField] float s_range = 3f;//강공 사거리
    [SerializeField] float s_stayCouont = 0.5f; //강공 경직 시간

    [Header("특공")]
    [SerializeField] float sp_drvieDecrease = 100f; //특공 드라이브 감소
    [SerializeField] float sp_stayCount = 1f; //특공 경직 시간

    [Header("번아웃")]
    bool isBunOut = false; //번아웃인가?


    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }
    void Start()
    {
        driveGauge = dg_max;   
    }

    void Update()
    {
        //번아웃 true 전환
        if(driveGauge <= 0 && isBunOut == false )
        {
            isBunOut = true;
            ForceStopDefense();//번아웃 시 강제로 방어 헤제
        }

       
        WeakAttack();
        StrongAttack();
        SpecialAttack();
        
    }
 
    public void OnDefense(InputValue value) //방어키 입력
    {
        if (value.isPressed)
        {
            if(!isBunOut&& defenseCoroutine == null)
            {
                defenseCoroutine = StartCoroutine(Defense());//defense 코루틴 시작
            }
        }
        else //방어키를 입력을 안하고 있을 때
        {
            if (defenseCoroutine != null) 
            {
                StopCoroutine(defenseCoroutine);
                defenseCoroutine = null;
            }
            StartCoroutine(EndDefense()); //종료 코루틴 시작
        }
    }
    private void ForceStopDefense() //강제 방어 종료
    {
        if (defenseCoroutine != null)
        {
            StopCoroutine(defenseCoroutine); //현재 진행중이 방어를 종료
            defenseCoroutine = null; //코루틴 변수 비워줌
            StartCoroutine(EndDefense()); //방어 종료 코루틴 시작
        }
    }
    IEnumerator Defense() //방어
    {      
        yield return new WaitForSeconds(d_startDelay);//방어 시작 딜레이
        if (movement != null) movement.SetDefending(true); //방어 true알람
        //만약 방어 중에 상대의 공격을 방어했다면 
        if (isdefense)
        {
          DecreaseDriveGauge(d_driveDecease);          
        }
    }
    IEnumerator EndDefense() //방어 종료
    {      
        yield return new WaitForSeconds(d_endDelay);//방어 해제 딜레이
        if (movement != null) movement.SetDefending(false); //방어 false알림  
    }

    void WeakAttack()//약공
    {
        w_timer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Z) && w_timer>=w_attackrange)
        {

        }
    }
    void StrongAttack() //강공
    {
        if (Input.GetKeyDown(KeyCode.X))
        {

        }
    }

    void SpecialAttack() //특공
    {
        if (Input.GetKeyDown(KeyCode.V) && isBunOut == false)
        {
            driveGauge -= sp_drvieDecrease;
        }
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
        if(driveGauge >= dg_max) //드라이브 게이지가 최대 드라이브 게이지보다 크거나 같으면
        {
            isBunOut = false; //isBunOut을 false로 변경
        }
        regenCoroutine = null;
    }

    //드라이브 게이지 감소 함수
    public void DecreaseDriveGauge(float amount)
    {
        driveGauge = (driveGauge - amount) <= 0 ? 0 : (driveGauge - amount); //드라이브게이지 감소
        if(regenCoroutine != null) //실행 중인 코루틴이 있다면 
        {
            StopCoroutine(regenCoroutine); //멈추게 함
        }
        regenCoroutine = StartCoroutine(RegenDriveGauge()); //코루틴 시작
    }

    // 약공이나 강공을 적중 시키면 드라이브게이지 회복 
    public void HealthSomeOfDriveGauge(float amount) {

        driveGauge = (driveGauge + amount) >= dg_max ? dg_max : (driveGauge + amount);    
        if(driveGauge >= dg_max)
        {
            isBunOut = false;
        }
    }


    //적으로부터 드라이브 게이지의 수치를 입력 받는 함수
    void GetDecreaseDriveGuauge(float amount)
    {
        d_driveDecease = amount;
    }
}
