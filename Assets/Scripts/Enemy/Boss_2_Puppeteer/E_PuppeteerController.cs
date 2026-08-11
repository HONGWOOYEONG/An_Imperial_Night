using UnityEngine;

public class E_PuppeteerController : MonoBehaviour
{
    public IPuppeteerState currentState;
    public IPuppeteerState chooseState;
    Rigidbody2D rb;

    [Header("상태")]
    public IPuppeteerState idle;
    public IPuppeteerState groggy;
    public IPuppeteerState move;
    public IPuppeteerState A;
    public IPuppeteerState B;
    public IPuppeteerState C;
    public IPuppeteerState D;
    public IPuppeteerState E;


    [Header("상태 쿨타임")]
    [SerializeField] float coolTime_A = 15f;
    [SerializeField] float coolTime_B = 17f;
    [SerializeField] float coolTime_C = 20f; //아직 정해지지않음 임의의 값
    [SerializeField] float coolTime_D = 12f;
    [SerializeField] float coolTime_E = 13f;
    //쿨타임을 위한 변수
    public bool didState_A = false;
    public bool didState_B = false;
    public bool didState_C = false; 
    public bool didState_D = false;
    public bool didState_E = false;
    float next_A = 0f;
    float next_B = 0f;
    float next_C = 0f;
    float next_D = 0f;
    float next_E = 0f;

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
    [HideInInspector] public bool isCase_1 = false; //케이스 1과 2를 구분하기 위한 변수
    [HideInInspector] public bool isRaged = false;
    [HideInInspector] public int countAtk = 0;

    [Header("D")]
    [HideInInspector] public bool isFar = false; //회월이랑 태자랑 먼지 알기위한 변수
    [HideInInspector] public Vector2 middlePoint = Vector2.zero;

    [Header("E")]
    public GameObject HitBox_E;
    [HideInInspector] public bool isAttaking_E = false;

    void Start()
    {
        idle = new P_IdleState();
        groggy = new P_GroggyState();
        move = new P_MoveState();
        A = new P_PatternA_State();
        B = new P_PatternB_State();
        C = new P_PatternC_State();
        D = new P_PatternD_State();
        E = new P_PatternE_State();

        rb = GetComponent<Rigidbody2D>();
        currentGroggy = 0f;
        currentHelth = maxHealth;

        FindPlayers();
        ChangeState(idle); //첫 시작할 때 Idle 상태로 변환   
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
            ChangeState(groggy);
        }

        //상태 쿨타임 해제
        if (didState_A && Time.time > next_A)
        {
            didState_A = false;
        }
        if (didState_B && Time.time > next_B)
        {
            didState_B = false;
        }
        if (didState_C && Time.time > next_C)
        {
            didState_C = false;
        }
        if (didState_D && Time.time > next_D)
        {
            didState_D = false;
        }
        if (didState_E && Time.time > next_E)
        {
            didState_E = false;
        }
        currentState?.Update(this);
    }


    public void ChangeState(IPuppeteerState _state)
    {
        if (currentState == _state) return;
        if (_state == null)
        {
            if (idle != null && currentState != idle)
            {
                ChangeState(idle);
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
        //if(currentState is  RunLikeHorseState)
        //{
        //    didStateRLH = true;
        //    next_RLH = Time.time + coolTime_state;
        //}
        if (currentState is P_PatternB_State)
        {
            didState_B = true;
            next_B = Time.time + coolTime_B;
        }
        else if (currentState is P_PatternA_State)
        {
            didState_A = true;
            next_A = Time.time + coolTime_A;
        }
        else if (currentState is P_PatternC_State)
        {
            didState_C = true;
            next_C = Time.time + coolTime_C;
        }
        else if (currentState is P_PatternD_State)
        {
            didState_D = true;
            next_D = Time.time + coolTime_D;
        }
        else if (currentState is P_PatternE_State)
        {
            didState_E = true;
            next_E = Time.time + coolTime_E;
        }
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
