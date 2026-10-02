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

    // base.Tick을 부르지 않는다. move는 루프 클립이라 '한 바퀴 돌았는가'가 의미를 갖지 않고,
    // base가 올리는 타이머를 이 상태는 읽지도 않는다. Idle과 같은 구조로 둔다.
    // 이 상태를 벗어나는 판단은 전부 Boss_AI가 한다.
    public override void Tick()
    {
        boss.AI.Pattern_Decide();
    }
}
