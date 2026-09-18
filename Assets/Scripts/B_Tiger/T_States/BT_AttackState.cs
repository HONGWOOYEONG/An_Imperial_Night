using UnityEngine;

public class BT_AttackState : IBossState
{
    private BossPatternData currentPattern;

    public void Enter(B_TigerFSM fsm)
    {
        currentPattern = fsm.TigerUtilityAI.SelectPattern();

        if (currentPattern == null)
        {
            fsm.ChangeState(fsm.IdleState);
            return;
        }

        fsm.Animator.SetInteger("PatternNum", currentPattern.patternNum);
        fsm.Animator.SetTrigger("Attack");

        PlayerContext target =  fsm.TigerUtilityAI.GetCurrentTarget();
        fsm.PatternExecutor.Execute(currentPattern, target);
    }

    public void Update(B_TigerFSM fsm)
    {
        if (!fsm.PatternExecutor.IsRunning)
        {
            fsm.ChangeState(fsm.IdleState);
        }
    }

    public void Exit(B_TigerFSM fsm)
    {
        fsm.PatternExecutor.Stop();
        currentPattern = null;
    }

}
