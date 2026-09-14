/// 작성자 : 유희일
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대기·이동·공격 중 무엇을 할지 정하고 상태를 바꾼다. 패턴 리스트를 소유하는 유일한 지점이다.
/// MonoBehaviour가 아니다. Boss_Controller가 생성해서 들고 있는다.
///
/// 여기 두지 않는 것
/// - 그로기·사망 전환 : Boss_Health의 이벤트를 Boss_Controller가 받아서 바꾼다.
/// - 패턴 실행 : 고르기만 하고, 실제 재생은 Boss_AttackState가 한다.
/// </summary>
public class Boss_AI
{
    private readonly Boss_Controller boss;
    private readonly Boss_Context context;
    private readonly List<Boss_PatternSO> patterns;

    // 판단 사이의 최소 간격. 0이면 공격이 끝나자마자 다음 공격이 붙어서 쉴 틈이 없어진다.
    private readonly float decideInterval;

    // 그로기가 유지되는 시간. 값은 여기서 들고, 끝났는지 판단도 Decide에서 한다.
    private readonly float groggyDuration;

    // 룰렛을 돌릴 때마다 배열을 새로 잡지 않도록 점수 버퍼를 재사용한다.
    private readonly float[] scores;

    // 비교형 타이머. 게임플레이 시계를 쓴다.
    private float nextDecideTime;
    private float groggyEndTime;

    public Boss_AI(Boss_Controller boss, Boss_Context context, List<Boss_PatternSO> patterns, float decideInterval, float groggyDuration)
    {
        this.boss = boss;
        this.context = context;
        this.patterns = patterns;
        this.decideInterval = decideInterval;
        this.groggyDuration = groggyDuration;

        scores = new float[patterns == null ? 0 : patterns.Count];
    }

    public Boss_PatternSO GetPattern(int index)
    {
        if (patterns == null) return null;
        if (index < 0 || index >= patterns.Count) return null;

        return patterns[index];
    }

    /// <summary>
    /// 그로기가 시작된 시점을 기록한다. Boss_GroggyState가 진입하면서 부른다.
    /// </summary>
    public void Begin_Groggy()
    {
        groggyEndTime = Time.time + groggyDuration;
    }

    /// <summary>
    /// Boss_IdleState, Boss_MoveState, Boss_GroggyState가 자기 Tick에서 부른다.
    /// 공격·사망 상태에서는 호출되지 않으므로, 패턴 도중에 판단이 끼어들 수 없다.
    /// </summary>
    public void Decide()
    {
        // 그로기 중에는 다른 판단을 하지 않는다. 끝났는지만 본다.
        // decideInterval 게이트보다 먼저 두어야 그로기 해제가 그 간격만큼 밀리지 않는다.
        if (boss.FSM.Current == boss.FSM.Groggy)
        {
            if (Time.time < groggyEndTime) return;

            // 밸런스를 되돌리고 나간다. 0인 채로 나가면 다음 피격에 곧바로 다시 그로기다.
            boss.Health.Init_Balance();
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }

        if (Time.time < nextDecideTime) return;

        nextDecideTime = Time.time + decideInterval;

        // 타깃이 없으면 판단할 근거가 없다. 거리 조건이 전부 무의미해지므로 대기로 돌린다.
        if (!context.HasTarget)
        {
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }

        int index = Select_PatternIndex();

        // 전부 쿨타임이거나 사거리 밖이다. 걸어 들어가서 다시 판단한다.
        if (index == Boss_Context.NoPattern)
        {
            boss.FSM.ChangeState(boss.FSM.Move);
            return;
        }

        context.SetNextPattern(index);
        boss.FSM.ChangeState(boss.FSM.Attack);
    }

    // 점수를 가중치로 삼는 룰렛이다. 점수가 높을수록 자주 뽑히지만 항상 뽑히지는 않는다.
    private int Select_PatternIndex()
    {
        if (patterns == null) return Boss_Context.NoPattern;

        float totalWeight = 0f;

        for (int i = 0; i < patterns.Count; i++)
        {
            scores[i] = Evaluate_Pattern(patterns[i]);
            totalWeight += scores[i];
        }

        if (totalWeight <= 0f) return Boss_Context.NoPattern;

        float pick = Random.Range(0f, totalWeight);

        for (int i = 0; i < patterns.Count; i++)
        {
            pick -= scores[i];

            if (pick <= 0f) return i;
        }

        // 부동소수 오차로 마지막까지 떨어지지 않는 경우가 있다. 그때는 마지막 후보를 준다.
        return patterns.Count - 1;
    }

    private float Evaluate_Pattern(Boss_PatternSO pattern)
    {
        if (pattern == null) return 0f;

        if (context.IsOnCooldown(pattern.Id, pattern.Cooldown)) return 0f;

        float distance = context.DistanceToTarget;

        if (distance < pattern.MinDistance || distance > pattern.MaxDistance) return 0f;

        float range = pattern.MaxDistance - pattern.MinDistance;

        // min과 max가 같으면 나눌 수가 없다. 데이터 실수이므로 후보에서 뺀다.
        if (range <= 0f) return 0f;

        float distanceScore = 1f - Mathf.Abs(distance - pattern.PreferredDistance) / range;

        // preferredDistance를 min~max 밖에 적어두면 점수가 음수로 나온다.
        // 음수를 그대로 더하면 룰렛의 총합이 줄어 엉뚱한 패턴이 뽑힌다.
        return pattern.BaseWeight * Mathf.Max(0f, distanceScore);
    }
}
