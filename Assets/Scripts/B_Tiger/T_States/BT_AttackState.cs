using UnityEngine;

public class BT_AttackState : IBossState
{
    private BossPatternData currentPattern;

    public void Enter(B_TigerFSM fsm)
    {
        currentPattern = fsm.TigerUtilityAI.SelectPattern();
    }

    public void Update(B_TigerFSM fsm)
    {
        //patternExecutor(currentPattern); 
    }

    public void Exit(B_TigerFSM fsm)
    {
        currentPattern = null;
    }
}
