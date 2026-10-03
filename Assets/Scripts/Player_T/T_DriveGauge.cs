using UnityEngine;
using System.Collections;

public class T_DriveGauge : MonoBehaviour
{
    private T_Defence t_Defence = null;

    [Header("드라이브 게이지")]
    public float driveGauge = 1000f;
    public float dg_max = 1000f; //드라이브 게이지 최대치
    [SerializeField] private const float dg_health = 50f; //드라이브 초당 회복량
    [SerializeField] float dg_delay = 3f; //드라이브 회복 시작 지연시간
    private Coroutine regenCoroutine = null;

    [Header("번아웃")]
    public bool isBunOut = false; //번아웃인가?

    private void Awake()
    {
        driveGauge = dg_max;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        t_Defence = GetComponent<T_Defence>(); 
    }

    // Update is called once per frame
    void Update()
    {
  
    }
   

    //일정 시간마다 드라이브 게이지 회복 
    IEnumerator RegenDriveGauge()
    {
        yield return new WaitForSeconds(dg_delay);

        while (driveGauge < dg_max) 
        {
            driveGauge += dg_health * Time.deltaTime; //드라이브 게이지 초당 회복
            Debug.Log("드라이브 게이지 초당 회복 : "+ driveGauge);
            yield return null;
        }
        CheckBurnOutFalse();
        regenCoroutine = null;
    }

    //드라이브 게이지 감소 함수
    public void DecreaseDriveGauge(float amount)
    {
        driveGauge = Mathf.Clamp(driveGauge - amount,0, dg_max);
        Debug.Log("드라이브게이지 감소 : " + driveGauge);
        CheckBurnOutTrue();

        //--게이지가 감소할때 잠깐 멈추게 하기 위함
        if (regenCoroutine != null) 
        {
            StopCoroutine(regenCoroutine); 
        }
        regenCoroutine = StartCoroutine(RegenDriveGauge()); 
    }

    // 약공이나 강공을 적중 시키면 드라이브게이지 회복 
    public void HealthSomeOfDriveGauge(float amount)
    {
        driveGauge = Mathf.Clamp(driveGauge + amount , 0, dg_max);
        CheckBurnOutFalse();
    }
    private void CheckBurnOutTrue()
    {
        //번아웃 true 전환 (진입)
        if (driveGauge <= 0 && isBunOut == false)
        {
            isBunOut = true;
            t_Defence.ForceStopDefense();//번아웃 시 강제로 방어 헤제
        }
    }
    private void CheckBurnOutFalse()
    {
        if (driveGauge >= dg_max && isBunOut == true) //한 번 번아웃이 되고 드라이브게이지가 1000이 됐다면 번아웃 해제
        {
            isBunOut = false; //번아웃 해제
        }
    }

   
   
}
