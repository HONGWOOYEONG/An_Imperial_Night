using UnityEngine;

public class BT_GroggyState : IBossState
{
    private const float GroggyTime = 3f;

    public void Enter(B_TigerFSM fsm)
    {
        fsm.TigerController.Stop();

        fsm.stateExecutor.CallStateCoroutine(GroggyTime);

    }

    public void Update(B_TigerFSM fsm)
    {
        if (fsm.stateExecutor.IsCoroutineRunning)
            return;

        fsm.TigerController.EndGroggy();
        fsm.ChangeState(fsm.IdleState);
    }

    public void Exit(B_TigerFSM fsm)
    {
        
    }
}