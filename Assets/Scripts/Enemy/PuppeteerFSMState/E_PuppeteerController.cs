using UnityEngine;

public class E_PuppeteerController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public IEnemyState currentState;
    public IEnemyState chooseState;
    Rigidbody2D rb;

    [Header("상태")]
    public IEnemyState idle;
    public IEnemyState groggy;
    public IEnemyState move;
    public IEnemyState A;
    public IEnemyState B;
    public IEnemyState C;
    public IEnemyState D;
    public IEnemyState E;


    [Header("상태 쿨타임")]
    [SerializeField] float coolTime_A = 15f;
    [SerializeField] float coolTime_B = 17f;
    [SerializeField] float coolTime_C = 0f;
    [SerializeField] float coolTime_D = 12f;
    [SerializeField] float coolTime_E = 13f;
    //쿨타임을 위한 변수
    public bool didState_B = false; //JumpToRangedDealer State
    public bool didState_A = false; //DollRush State
    public bool didState_C = false; //RunLikeHorse
    public bool didState_D = false;
    public bool didState_E = false;
    float next_B = 0f;
    float next_A = 0f;
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
    public GameObject attackHitBox; //히트박스
    public Collider2D moveBox;
    public Collider2D hitbox;

    public Enemy_DollHitBox dollHitBox;
    [SerializeField] float detectRange = 10f; //감지 사거리
    [HideInInspector] public Collider2D[] colliders;
    [HideInInspector] public Collider2D targetPlayer;

    [HideInInspector] public Collider2D rangedDealer; //원거리 딜러
    [HideInInspector] public Collider2D damageDealer; //근거리 딜러
    public bool isInTargetPlayer = false;


    [Header("공격")]
    public bool isAttaking = false;

    [Header("A : DollRush")]
    public Transform dollTransform;

    [Header("D : ")]
    [HideInInspector] public bool isFar = false;

    void Start()
    {
        idle = new IdleState();
        groggy = new GroggyState();
        move = new MoveState();
        A = new PatternA_State();
        B = new PatternB_State();
        C = new PatternC_State();
        D = new PatternD_State();
        E = new PatternE_State();

        rb = GetComponent<Rigidbody2D>();
        currentGroggy = 0f;
        currentHelth = maxHealth;

        FindPlayers();
        targetPlayer = damageDealer;
        ChangeState(D); //첫 시작할 때 Idle 상태로 변환   
    }

    void Update()
    {
        FindPlayers(); //매 프레임마다 적 감지를 해줌
        //Flip();

        if (currentHelth <= 0f)
        {
            ChangeState(new DeadState());
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


    public void ChangeState(IEnemyState _state)
    {
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
        if (currentState is PatternB_State)
        {
            didState_B = true;
            next_B = Time.time + coolTime_B;
        }
        else if (currentState is PatternA_State)
        {
            didState_A = true;
            next_A = Time.time + coolTime_A;
        }
        else if (currentState is PatternC_State)
        {
            didState_C = true;
            next_C = Time.time + coolTime_C;
        }
        else if (currentState is PatternD_State)
        {
            didState_D = true;
            next_D = Time.time + coolTime_D;
        }
        else if (currentState is PatternE_State)
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
                damageDealer = collider;
            }
            else if (collider.gameObject.CompareTag("RangedDealer")) //원딜라면
            {
                rangedDealer = collider;
            }
        }

    }

    void Flip() //좌우
    {
        if (targetPlayer != null)
        {
            // targetPlayer의 X가 내 X보다 작으면 왼쪽, 크면 false오른쪽
            spriteRenderer.flipX = (targetPlayer.transform.position.x > transform.position.x);
        }
    }


    //감지 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(gameObject.transform.position, detectRange);
    }
}
