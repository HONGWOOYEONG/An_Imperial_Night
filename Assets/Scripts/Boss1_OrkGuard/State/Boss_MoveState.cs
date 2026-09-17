/// 작성자 : 유희일
using UnityEngine;

public class Boss_MoveState : Boss_Statebase
{
    public Boss_MoveState(Boss_Controller boss, string animParamName) : base(boss, animParamName) { }

    // 바라보는 방향으로만 걷는다. 멈추는 것은 Boss_Statebase의 기본 FixedTick이 맡으므로
    // 이 상태를 벗어나면 따로 멈추는 호출이 필요 없다.
    public override void FixedTick()
    {
        boss.Moter.Move_FixedTick();
    }
}
