/// 작성자 : 유희일
using UnityEngine;

// 판단은 Boss_AI.Tick이 한다. 여기서 Pattern_Decide를 부르면 매 프레임 nextDecideTime이 밀려 영영 판단하지 못한다.
public class Boss_IdleState : Boss_Statebase
{
    public Boss_IdleState(Boss_Controller boss, string animParamName) : base(boss, animParamName) { }

    override public void Tick()
    {
        // timer는 base.Tick이 올린다. 빠뜨리면 timer가 0에 묶여 Move로 영영 못 간다.
        base.Tick();
        // 거리 조건이 없으면 가까이 있어도 Move로 갔다가 MoveState가 바로 Idle로 돌려보내 둘이 번갈아 튄다.
        if (timer > 0.2f && 
            boss.Context.DistanceToTarget > boss.Moter.StopDistance)
        {
            boss.Moter.HandleFlip();
            boss.FSM.ChangeState(boss.FSM.Move);
        }
    }

    public override void Exit()
    {
        base.Exit();
        
    }
}
