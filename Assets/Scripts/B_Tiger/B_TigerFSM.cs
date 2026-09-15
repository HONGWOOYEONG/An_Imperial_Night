using JetBrains.Annotations;
using UnityEngine;

public class B_TigerFSM : MonoBehaviour
{
    public IBossState currentBossState;
    public StateExecutor stateExecutor;
    [SerializeField] private BT_PatternExecutor patternExecutor;
    private B_TigerController tigerController;
    private BT_TargetDetector targetDetector;
    private B_TigerUtilityAI tigerUtilityAI;
    private Animator animator;


    [Header("State")]
    private IBossState idleState;
    private IBossState moveState;
    private IBossState deathState;
    private IBossState attackState;
    private IBossState groggyState;

    public IBossState IdleState => idleState;
    public IBossState MoveState => moveState;  
    public IBossState DeathState => deathState;
    public IBossState AttackState => attackState;
    public IBossState GroggyState => groggyState;

    public BT_TargetDetector TargetDetector => targetDetector;
    public B_TigerController TigerController => tigerController;
    public bool IsTargetDetected => targetDetector.IsTargetDetected;
    public B_TigerUtilityAI TigerUtilityAI => tigerUtilityAI;
    public BT_PatternExecutor PatternExecutor => patternExecutor;
    public Animator Animator => animator;

    void Start()
    {
        stateExecutor = GetComponent<StateExecutor> ();
        patternExecutor = GetComponent<BT_PatternExecutor>();
        tigerController = GetComponent<B_TigerController>();
        targetDetector = GetComponent<BT_TargetDetector>();
        tigerUtilityAI = GetComponent<B_TigerUtilityAI>();
        animator = GetComponent<Animator>();

        idleState = new BT_IdleState();
        attackState = new BT_AttackState();
        deathState = new BT_DeathState();
        groggyState = new BT_GroggyState();
        moveState = new BT_MoveState();

        ChangeState(idleState);
    }


    public void ChangeState(IBossState nextState)
    {
        if (nextState == null)
        {
            return;
        }
        currentBossState?.Exit(this);
        currentBossState = nextState;
        currentBossState.Enter(this);
    }

    void Update()
    {
        currentBossState?.Update(this);
    }
}
