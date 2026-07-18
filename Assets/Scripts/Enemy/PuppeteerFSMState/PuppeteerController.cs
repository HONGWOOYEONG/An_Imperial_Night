using UnityEngine;

public class PuppeteerController : MonoBehaviour
{
    public IEnemyState currentState;
    public IEnemyState chooseState;
    Rigidbody2D rb;

    [Header("상태 쿨타임")]
    [SerializeField] float coolTime_state = 15f;
    //쿨타임을 위한 변수
    public bool didStateJTRD = false; //JumpToRangedDealer State
    public bool didStateDR = false; //DollRush State
    public bool didStateRLH = false; //RunLikeHorse
    float next_JTRD = 0f;
    float next_DR = 0f;
    float next_RLH = 0f;

    [Header("체력")]
    [SerializeField] float maxHealth = 1000f;
    [SerializeField] float currentHelth = 0f;

    [Header("체간")]
    [SerializeField] float maxGroggy = 100f;
    [SerializeField] float currentGroggy;
    [SerializeField] bool isGroggy = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentGroggy = 0f;

        ChangeState(new IdleState()); //첫 시작할 때 Idle 상태로 변환   
    }

    void Update()
    {
        if(currentGroggy >= maxGroggy)
        {
            isGroggy = true;
        }
        if (isGroggy) //그로기수치 이상이 되면 강제로 그로기 상태로 변환
        {
            ChangeState(new GroggyState());
        }
        
        //상태 쿨타임
        //if(didStateRLH && Time.time > next_RLH)
        //{
        //    didStateRLH = false;
        //}
        if(didStateJTRD && Time.time > next_JTRD)
        {
            didStateJTRD = false;
        }
        if(didStateDR && Time.time > next_DR)
        {
            didStateDR = false;
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
        if(currentState is  RunLikeHorseState)
        {
            didStateRLH = true;
            next_RLH = Time.time + coolTime_state;
        }
        else if(currentState is JumpToRangedDealerState)
        {
            didStateJTRD = true;
            next_JTRD = Time.time + coolTime_state;
        }
        else if(currentState is DollRushState)
        {
            didStateDR = true;
            next_DR = Time.time + coolTime_state;
        }
    }

    
}
