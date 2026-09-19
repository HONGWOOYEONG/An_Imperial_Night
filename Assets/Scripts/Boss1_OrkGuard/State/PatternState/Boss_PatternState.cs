/// 작성자 : 유희일
/// 모든 패턴이 공통적으로 지나가는 상태.
/// 패턴마다 동작 방식이 다르므로 애니메이션이벤트로 호출한다.
/// 패턴의 종료 또한 이벤트로 호출.
using UnityEngine;

public class Boss_PatternState : Boss_Statebase
{
    private Boss_PatternSO pattern;

    public Boss_PatternState(Boss_Controller boss) : base(boss, string.Empty) { }

    public void Set_Pattern(Boss_PatternSO next)
    {
        pattern = next;
    }

    public override void Enter()
    {
        if (pattern == null)
        {
            Debug.LogWarning($"{boss.name} : 재생할 클립이 없는 패턴이다. Boss_PatternSO의 clips를 채워야 실행된다.", boss);
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }
        
        boss.AI.Pattern_Start();
        boss.Moter.HandleFlip();
    }
    // 각 패턴의 종료는 애니메이션 이벤트로 받는다. changeState
    public override void Exit()
    {
        base.Exit();

        boss.AI.Delay_NextDecide();
    }
}
