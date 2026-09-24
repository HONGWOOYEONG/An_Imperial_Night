/// 작성자 : 유희일
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 대기·이동·공격 중 무엇을 할지 정하고 상태를 바꾼다. 패턴 리스트를 소유하는 유일한 지점이다.
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
    private Boss_PatternSO currentPattern;


    // 판단 사이의 최소 간격. 0이면 공격이 끝나자마자 다음 공격이 붙어서 쉴 틈이 없어진다.
    private readonly float decideInterval;

    // 그로기가 유지되는 시간. 값은 여기서 들고, 끝났는지 판단도 Decide에서 한다.
    private readonly float groggyDuration;

    // 룰렛을 돌릴 때마다 배열을 새로 잡지 않도록 점수 버퍼를 재사용한다.
    private readonly float[] scores;

    // 비교형 타이머. 게임플레이 시계를 쓴다.
    private float nextDecideTime = 1f;
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

    public void Begin_Groggy()
    {
        groggyEndTime = Time.time + groggyDuration;
    }

    /// <summary>
    /// 그로기를 푸는 일만 한다. 나머지 전환은 전부 Pattern_Decide가 한다.
    /// </summary>
    // 판단을 두 함수로 나눠 두었더니, 간격 검사는 이쪽에만 있고 실제 결정은 Pattern_Decide에만
    // 있어서 간격이 한 번도 적용되지 않았다. 결정하는 곳은 Pattern_Decide 하나로 모았다.
    public void Tick()
    {
        Pattern_Check();
        if (boss.FSM.Current != boss.FSM.Groggy) return;

        if (Time.time < groggyEndTime) return;

        boss.Health.Init_Balance();
        boss.FSM.ChangeState(boss.FSM.Idle);
    }

    /// <summary>
    /// 패턴이 끝난 시점에 Boss_PatternState가 부른다. 다음 판단을 decideInterval만큼 미뤄
    /// 패턴과 패턴 사이에 여백을 만든다.
    /// </summary>
    // 패턴이 도는 동안에는 Pattern_Decide가 아예 호출되지 않으므로, 패턴 시작 시점에 찍어둔
    // 다음 판단 시각은 패턴이 끝날 때쯤 이미 지나 있다. 끝나는 시점에 다시 찍어야 여백이 생긴다.
    public void Delay_NextDecide()
    {
        nextDecideTime = Time.time + decideInterval;
    }

    /// <summary>
    /// 지금 쓸 패턴을 고르고 상태를 바꾼다. Idle과 Move가 매 프레임 부른다.
    /// 보스의 전환을 결정하는 유일한 함수다(그로기 해제 제외).
    /// </summary>
    public void Pattern_Decide()
    {
        if (Time.time < nextDecideTime) return;
        nextDecideTime = Time.time + decideInterval;

        // 타깃이 없으면 걸어갈 곳도 때릴 상대도 없다. 이 검사를 Tick에 두었을 때는
        // Idle이 Move로 가고 Tick이 다시 Idle로 되돌리는 왕복이 한 프레임 안에서 일어났다.
        if (!context.HasTarget)
        {
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }

        currentPattern = Select_Pattern();
        // 공격 클래스에 값을 넘김.



        // 전부 쿨타임이거나 사거리 밖이다. 걸어 들어가서 다시 판단한다.
        if (currentPattern == null)
        {
            boss.FSM.ChangeState(boss.FSM.Move);
            return;
        }

        // 쿨다운 기록은 상태를 바꾸기 전에 남긴다. 뒤로 미루면 패턴이 진입 도중 되돌아왔을 때
        // 기록이 통째로 빠지고, 같은 패턴이 다음 판단에서 또 뽑혀 무한히 재진입한다.
        context.Record_PatternUsed(currentPattern.Id);
        context.SetPatternID(currentPattern.Id);
        boss.Anim.SetInteger("patternId",currentPattern.Id);

        // 넘기는 것이 먼저다. ChangeState부터 하면 Enter가 직전 패턴을 한 번 더 재생한다.
        boss.FSM.Pattern.Set_Pattern(currentPattern);
        boss.FSM.ChangeState(boss.FSM.Pattern);
    }

    private bool onPattern = false;
    public void Pattern_Check() => boss.Anim.SetBool("onPattern",onPattern);





    // 애니메이션 이벤트용 호출 함수.
    public void Pattern_Start()
    {
        onPattern = true;
    } 
    public void Pattern_End()
    {
        boss.FSM.ChangeState(boss.FSM.Idle);
        onPattern = false;
    }























    private Boss_PatternSO Select_Pattern()
    {
        if (patterns == null) return null;

        float totalWeight = 0f;

        for (int i = 0; i < patterns.Count; i++)
        {
            scores[i] = Evaluate_Pattern(patterns[i]);
            totalWeight += scores[i];
        }

        if (totalWeight <= 0f) return null;

        float pick = Random.Range(0f, totalWeight);

        for (int i = 0; i < patterns.Count; i++)
        {
            pick -= scores[i];

            if (pick <= 0f) return patterns[i];
        }

        return patterns[patterns.Count - 1];
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
