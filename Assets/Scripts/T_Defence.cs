using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class T_Defence : MonoBehaviour
{
    private PlayerMovement movement;
    public const float BASE_FPS = 30f;

    [Header("드라이브 게이지")]
    [SerializeField] float driveGauge = 0f;
    [SerializeField] float dg_max = 1000f; //드라이브 게이지 최대치
    [SerializeField] float dg_health = 50f; //드라이브 초당 회복량
    [SerializeField] float dg_delay = 3f; //드라이브 회복 시작 지연시간
    private Coroutine regenCoroutine;

    [Header("방어")]
    [SerializeField] float d_driveDecease = 0; //방어 시 감소하는 드라이브 게이지
    [SerializeField] float d_startDelay = 2f; //방어 시작 딜레이
    [SerializeField] float d_endDelay = 2f; //방어 해제 딜레이
    private Coroutine defenceCoroutine;

    //적이 공격을 하고 플레이어가 방어 중이라면 isdefense를 true로 변경
    public bool isdefence = false; //방어를 성공 했나?

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

    public void OnDefence(InputValue value) //방어키 입력
    {
        if (value.isPressed)
        {
            if (!isBunOut && defenceCoroutine == null)
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
            StartCoroutine(EndDefence()); //종료 코루틴 시작
        }
    }
    private void ForceStopDefense() //강제 방어 종료
    {
        if (defenceCoroutine != null)
        {
            StopCoroutine(defenceCoroutine); //현재 진행중이 방어를 종료
            defenceCoroutine = null; //코루틴 변수 비워줌
            StartCoroutine(EndDefence()); //방어 종료 코루틴 시작
        }
    }
    IEnumerator Defence() //방어
    {
        yield return new WaitForSeconds(FrameToSeconds(d_startDelay));//방어 시작 딜레이
        if (movement != null) movement.SetDefending(true); //방어 true알람


        //만약 방어 중에 상대의 공격을 방어했다면 
        if (isdefence)
        {
            DecreaseDriveGauge(d_driveDecease);
        }
    }
    IEnumerator EndDefence() //방어 종료
    {
        yield return new WaitForSeconds(d_endDelay);//방어 해제 딜레이
        if (movement != null) movement.SetDefending(false); //방어 false알림  
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


    //적으로부터 드라이브 게이지의 수치를 입력 받는 함수
    void GetDecreaseDriveGuauge(float amount)
    {
        d_driveDecease = amount;
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

    //적의 공격에 정확한 타이밍에 방어를 한다면 패링이 되고 드라이브 게이지가 회복이 된다.

    //방어를 시작 할 때 딜레이가 있고 방어 해제 할 때 딜레이가 있다. 
    //방어 중 이동 속도 배율이 있는데 이동 속도는 PlayerMovement 스크립트에서 지정
   

}
