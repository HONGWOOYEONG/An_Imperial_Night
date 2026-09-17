/// 작성자 : 유희일
using UnityEngine;

public class Boss_MoveState : Boss_Statebase
{
    public Boss_MoveState(Boss_Controller boss, string animParamName) : base(boss, animParamName) { }

    public override void Tick()
    {
        // 바라보는 방향으로만 걷는다. MoveTick을 부르지 않는 상태는 그 자체로 멈춘 것이라
        // 따로 멈추는 호출이 필요 없다.
        boss.Moter.MoveTick();

        boss.AI.Decide();
    }
}
