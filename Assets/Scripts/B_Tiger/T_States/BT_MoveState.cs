using UnityEngine;

public class BT_MoveState : IBossState
{
    PlayerContext targetContext;

    public void Enter(B_TigerFSM fsm)
    {

    }

    public void Update(B_TigerFSM fsm)
    {
        if (fsm.IsTargetDetected)
        {
            fsm.ChangeState(fsm.IdleState);
            return;
        }

        targetContext = fsm.TigerUtilityAI.GetTraceTarget();

        if(targetContext == null) return;

        fsm.TigerController.MoveTowartTarget(targetContext.getPosition());
    }

    public void Exit(B_TigerFSM fsm)
    {

    }
}
