using UnityEngine;

public class B_TigerFSM : MonoBehaviour
{
    public IBossState currentBossState;

    [Header("State")]
    private IBossState idleState;
    private IBossState moveState;
    private IBossState deathState;
    private IBossState attackState;
    private IBossState groggyState;

    void Start()
    {
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
