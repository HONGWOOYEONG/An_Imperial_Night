/// 작성자 : 유희일
using UnityEngine;

public class Boss_AttackState : Boss_Statebase
{
    private const string AttackIndexParam = "attackIndex";

    private readonly int attackIndexHash;

    private Boss_PatternSO pattern;

    // 재생 길이는 SSM에서 어떤 클립이 뽑히느냐에 따라 달라진다.
    // 손으로 적어둔 프레임 수를 쓰면 클립을 교체했을 때 이동과 애니메이션이 어긋난다.
    private bool measured;
    private float duration;
    private float elapsed;

    public Boss_AttackState(Boss_Controller boss, Animator anim) : base(boss, anim, "attack")
    {
        attackIndexHash = Animator.StringToHash(AttackIndexParam);
    }

    public override void Enter()
    {
        base.Enter();

        pattern = boss.AI.GetPattern(boss.Context.NextPatternIndex);

        measured = false;
        duration = 0f;
        elapsed = 0f;

        if (pattern == null) return;

        anim.SetInteger(attackIndexHash, pattern.AnimIndex);

        // 고른 시점이 아니라 실제로 시작한 시점에 기록한다.
        boss.Context.Record_PatternUsed(pattern.Id);
    }

    public override void Tick()
    {
        // 패턴이 비어 있으면 여기서 버틸 이유가 없다. 버티면 공격 상태에서 영영 못 빠져나온다.
        if (pattern == null)
        {
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }

        if (!measured)
        {
            // 전이 중에는 아직 이전 클립의 길이가 나온다. 그 값으로 재면 이동 속도가 통째로 틀어진다.
            if (anim.IsInTransition(0)) return;

            duration = anim.GetCurrentAnimatorStateInfo(0).length;
            measured = true;

            Begin_Move();
        }

        elapsed += Time.deltaTime;

        if (elapsed < duration) return;

        boss.FSM.ChangeState(boss.FSM.Idle);
    }

    public override void Exit()
    {
        base.Exit();

        // 패턴이 중간에 끊긴 경우를 위해 이동도 같이 끊는다.
        boss.Moter.CancelKinematicMove();
    }

    /// <summary>
    /// 애니메이션 길이보다 일찍 끊고 싶을 때 애니메이션 이벤트로 들어온다.
    /// </summary>
    public void OnPatternEnd()
    {
        // 이미 다른 상태로 넘어간 뒤 뒤늦게 도착한 이벤트는 무시한다.
        if (boss.FSM.Current != this) return;

        boss.FSM.ChangeState(boss.FSM.Idle);
    }

    private void Begin_Move()
    {
        if (duration <= 0f) return;
        if (pattern.MoveDistance == 0f && pattern.MoveHeight == 0f) return;

        // 애니메이션이 끝나는 순간 정확히 moveDistance만큼 가 있도록 속도를 역산한다.
        // 제자리 점프는 거리가 0이라 높이를 기준으로 낸다.
        float speed = pattern.MoveDistance != 0f
            ? Mathf.Abs(pattern.MoveDistance) / duration
            : pattern.MoveHeight * 2f / duration;

        boss.Moter.Jump(pattern.MoveHeight, pattern.MoveDistance, speed);
    }
}
