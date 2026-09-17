using UnityEngine;

public class BossPS_a2 : Boss_Statebase
{
    public BossPS_a2(Boss_Controller boss, string animParamName) : base(boss, animParamName)
    {
        endIntervalTime = 1f;
        
    }
    public override void Enter()
    {
        base.Enter();
        anim.SetTrigger("next");
    }
        public override void Tick()
    {
        base.Tick();

        if(timer > endIntervalTime)
        {
            anim.SetTrigger("next");
            boss.FSM.ChangeState( boss.FSM.A3);
        }
    }
}
