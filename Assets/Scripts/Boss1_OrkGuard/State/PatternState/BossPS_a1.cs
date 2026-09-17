using UnityEngine;

public class BossPS_a1 : Boss_Statebase
{
    public BossPS_a1(Boss_Controller boss, string animParamName) : base(boss, animParamName)
    {
        endIntervalTime = 1f;
    }
    public override void Enter()
    {
        base.Enter();
        boss.Anim.SetBool("onPattern",boss.AI.OnPattern);
        
        boss.Anim.SetInteger("patternId",boss.Context.PatternID);
        boss.Moter.Dash_Begin(10f);
    }
    // 돌진은 rb를 미는 일이라 물리 스텝에서만 돈다.
    public override void FixedTick()
    {
        // 끝났으면 Dash_FixedTick이 false를 돌려주고 스스로 속도를 0으로 만든다.
        boss.Moter.Dash_FixedTick();
    }

    public override void Tick()
    {
        base.Tick();

        if(timer > endIntervalTime)
        {
            anim.SetTrigger("next");
            boss.FSM.ChangeState( boss.FSM.A2);
        }
    }
}
