using UnityEngine;

public class BT_DeathState : IBossState
{
    private float destroyDelay = 5f;
    public void Enter(B_TigerFSM fsm)
    {
        fsm.TigerController.Stop();
        fsm.stateExecutor.Interrupt();  

        fsm.TargetDetector.enabled = false;
        Object.Destroy(fsm.gameObject, destroyDelay);
    }

    public void Update(B_TigerFSM fsm)
    {
        
    }

    public void Exit(B_TigerFSM fsm)
    {
        
    }
}
