/// 작성자 : 유희일
using UnityEngine;

public class Boss_ChangeHandState : Boss_Statebase
{
    private static readonly int ChangeHandStateHash = Animator.StringToHash("changeHand");

    public Boss_ChangeHandState(Boss_Controller boss, string animParamName) : base(boss, animParamName) { }

    // 손바꾸기가 연달아 걸리면(패턴 끝 → 손바꾸기 → 반대 손에 패턴 없음 → 또 손바꾸기)
    // changeHand bool이 false로 꺼졌다 켜지는 사이에 애니메이터가 한 프레임도 돌지 않는다.
    // 그러면 애니메이터는 changeHand 클립 끝 프레임에 머문 채 재진입하지 않고(Can Transition To Self 꺼짐),
    // 끝 이벤트 AE_Pattern_End가 다시 오지 않아 FSM이 ChangeHand에 영원히 갇힌다.
    // 그래서 진입할 때마다 클립을 처음부터 강제로 다시 튼다.
    public override void Enter()
    {
        base.Enter();
        anim.Play(ChangeHandStateHash, 0, 0f);
    }
}
