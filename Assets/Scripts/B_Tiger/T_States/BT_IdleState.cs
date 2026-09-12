using UnityEngine;

public class BT_IdleState : IBossState
{
    private float idleTime = Random.Range(1f, 3f);
    private float timer = 0f;

    private bool isEndTimer;

    public void Enter(B_TigerFSM fsm)
    {
        isEndTimer = false;
        // fsm.stateExecutor.SetAnimation("Idle");
    }

    public void Update(B_TigerFSM fsm)
    {
        if (!fsm.IsTargetDetected)
        {
            fsm.ChangeState(fsm.MoveState);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= idleTime)
        {
            isEndTimer = true;
            fsm.ChangeState(fsm.AttackState);
        }
    }

    public void Exit(B_TigerFSM fsm)
    {
        if(!isEndTimer) return;
        else 
        {
            timer = 0f;
            idleTime = Random.Range(1f, 3f);
        }
    }
}