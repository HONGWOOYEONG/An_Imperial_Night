using UnityEngine;

public interface IBossState
{
    public void Enter(B_TigerFSM fsm);
    public void Update(B_TigerFSM fsm);
    public void Exit(B_TigerFSM fsm);
}
