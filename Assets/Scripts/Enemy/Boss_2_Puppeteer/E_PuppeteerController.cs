using System;
using System.Collections.Generic;
using UnityEngine;

public class E_PuppeteerController : MonoBehaviour
{
    [SerializeField] public float BASE_FPS = 60f;
    [SerializeField] public IPuppeteerState currentState;
    [SerializeField] public IPuppeteerState chooseState;
    [SerializeField] public Rigidbody2D rb;

    [Header("상태")]
    public Dictionary<string, IPuppeteerState> states;


    [Header("상태 쿨타임")]
    public Dictionary<Type,float> cooldowns = new Dictionary<Type, float>();
    [SerializeField] float coolTime_A = 15f;
    [SerializeField] float coolTime_B = 17f;
    [SerializeField] float coolTime_C = 20f; //임의
    [SerializeField] float coolTime_D = 12f;
    [SerializeField] float coolTime_E = 13f;
    [SerializeField] float coolTime_F = 13f; //임의
    [SerializeField] float coolTime_G = 13f; //임의


    [Header("체력")]
    [SerializeField] float maxHealth = 1000f;
    [SerializeField] float currentHelth = 0f;

    [Header("체간")]
    [SerializeField] float maxGroggy = 100f;
    [SerializeField] float currentGroggy;
    [SerializeField] bool isGroggy = false;

    [Header("적 감지")]
    [SerializeField] float detectRange = 10f; //감지 사거리
    [HideInInspector] public Collider2D[] colliders;
    [HideInInspector] public Collider2D targetPlayer;

    [HideInInspector] public Collider2D rangedDealer; //원거리 딜러
    [HideInInspector] public Collider2D meleeDealer; //근거리 딜러
    public bool isInTargetPlayer = false;


    [Header("A")]
    public GameObject HitBox_A;
    [HideInInspector]public bool isAttaking_A = false;
    [HideInInspector] public int currentCount = 0;

    [Header("B")]
    public GameObject HitBox_B;
    public GameObject HitBox_AirB;
  //  public GameObject HitBox_LandB;
    [HideInInspector] public bool isAttaking_AirB = false;
    [HideInInspector] public bool isAttaking_B = false;
    [HideInInspector] public bool isAttaling_B = false;
    [HideInInspector] public int count = 0;

    [Header("C")]
    public float rangeToPlayer_C = 3f; //임의 

    [Header("D")]
    [HideInInspector] public bool isFar = false; //회월이랑 태자랑 먼지 알기위한 변수
    [HideInInspector] public Vector2 middlePoint = Vector2.zero;

    [Header("E")]
    public GameObject HitBox_E;
    [HideInInspector] public bool isAttaking_E = false;

    [Header("F")]
    public GameObject spiderWeb; //거미줄
    public GameObject spiderwebSwamp; //거미줄 늪
    public GameObject spiderwebPillar; //거미줄 기둥
    public Transform throwFire; //거미줄 던지는 위치
    [HideInInspector]public GameObject leftSpiderWeb;
    [HideInInspector]public GameObject rightSpiderWeb;
    [HideInInspector] public GameObject leftSpiderWebSwamp;
    [HideInInspector] public GameObject rightSpiderWebSwamp;
    [HideInInspector] public bool isPatternEnded_F = false;
    public float rangeToPlayer_F = 3f; //n1 (임의)
    public float enemyRange = 6f; //n2 (임의)

    [Header("G")]
    public float minRange = 2f; //n1 이상 (임의)
    public float maxRange = 4f; //n2 이하 (임의)


    void Start()
    {
        states = new Dictionary<string, IPuppeteerState>{
            { "idle", new P_IdleState() },
            { "groggy", new P_GroggyState() }, 
            { "move", new P_MoveState() }, 
            { "A", new P_PatternA_State() }, 
            { "B", new P_PatternB_State() }, 
            { "C", new P_PatternC_State() },
            { "D", new P_PatternD_State() },
            { "E", new P_PatternE_State() },
            { "F", new P_PatternF_State() },
            { "G", new P_PatternG_State() }
        };


        rb = GetComponent<Rigidbody2D>();
        currentGroggy = 0f;
        currentHelth = maxHealth;

        FindPlayers();
        ChangeState(states["idle"]); //첫 시작할 때 Idle 상태로 변환   
    }

    void Update()
    {
        FindPlayers(); //매 프레임마다 적 감지를 해줌

        if (currentHelth <= 0f)
        {
            ChangeState(new P_DeadState());
        }
        if (currentGroggy >= maxGroggy)
        {
            isGroggy = true;
        }
        if (isGroggy) //그로기수치 이상이 되면 강제로 그로기 상태로 변환
        {
            ChangeState(states["groggy"]);
        }

        //상태 쿨타임 해제
      
        currentState?.Update(this);
    }


    public void ChangeState(IPuppeteerState _state)
    {
        if (currentState == _state) return;
        if (_state == null)
        {
            if (states["idle"] != null && currentState != states["idle"])
            {
                ChangeState(states["idle"]);
            }
            return;
        }
        if (currentState != null)
        {
            currentState.Exit(this); //이전 상태의 Exit 실행
            SetState(); //실행했던 state 쿨타임적용
        }
        currentState = _state; //현재 state를 넣어줌
        currentState.Enter(this); //현재 상태의 Enter 실행
    }

    void SetState() //쿨타임 시작
    {
        cooldowns[currentState.GetType()] = Time.time + GetCooldown(currentState);
    }
    float GetCooldown(IPuppeteerState state)
    {
        if (currentState is P_PatternA_State)
        {
            return coolTime_A;
        }
        if (currentState is P_PatternB_State)
        {
            return coolTime_B;
        }
        if (currentState is P_PatternC_State)
        {
            return coolTime_C;
        }
        if (currentState is P_PatternD_State)
        {
            return coolTime_D;
        }
        if (currentState is P_PatternE_State)
        {
            return coolTime_E;
        }
        if (currentState is P_PatternF_State)
        {
            return coolTime_F;
        }
        if (currentState is P_PatternG_State)
        {
            return coolTime_G;
        }
        return 0f;
    }
    void FindPlayers()
    {

        colliders = Physics2D.OverlapCircleAll(transform.position, detectRange);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag("DamageDealer")) //근딜이라면
            {
                meleeDealer = collider;
            }
            else if (collider.gameObject.CompareTag("RangedDealer")) //원딜라면
            {
                rangedDealer = collider;
            }
        }

    }

    public void LookAtLocation(float targetX)
    {
        bool isTargetRight = targetX < transform.position.x; //타겟이 오른쪽에 있으면 true 반환
        float yAngle = isTargetRight ? 0 : 180;
        transform.eulerAngles = new Vector2(0, yAngle);
    }


    //감지 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(gameObject.transform.position, detectRange);
    }
}
