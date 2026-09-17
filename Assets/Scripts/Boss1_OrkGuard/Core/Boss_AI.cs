/// 작성자 : 유희일
using System.Collections.Generic;
using UnityEngine;

// using System;가 같이 있으면 Random이 System.Random과 UnityEngine.Random 사이에서
// 모호해져 CS0104로 컴파일이 깨졌다. 별칭으로 못 박아두면 다시 들어와도 안전하다.
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
    // 고른 패턴을 인덱스가 아니라 데이터 자체로 들고 있는다. 인덱스를 들고 있던 동안
    // switch가 리스트 순서를 id로 착각해 pattern_A가 통째로 default로 빠졌다.
    private Boss_PatternSO currentPattern;


    // 판단 사이의 최소 간격. 0이면 공격이 끝나자마자 다음 공격이 붙어서 쉴 틈이 없어진다.
    private readonly float decideInterval;

    // 그로기가 유지되는 시간. 값은 여기서 들고, 끝났는지 판단도 Decide에서 한다.
    private readonly float groggyDuration;

    // 룰렛을 돌릴 때마다 배열을 새로 잡지 않도록 점수 버퍼를 재사용한다.
    private readonly float[] scores;

    // 비교형 타이머. 게임플레이 시계를 쓴다.
    private float nextDecideTime=0f;
    private float groggyEndTime;
    public bool OnPattern{get; private set;} = false;

    public Boss_AI(Boss_Controller boss, Boss_Context context, List<Boss_PatternSO> patterns, float decideInterval, float groggyDuration)
    {
        this.boss = boss;
        this.context = context;
        this.patterns = patterns;
        this.decideInterval = decideInterval;
        this.groggyDuration = groggyDuration;

        scores = new float[patterns == null ? 0 : patterns.Count];

        Validate_PatternIds();

        
    }

    /// <summary>
    /// id로 패턴 데이터를 찾는다. 인덱스로 찾지 않으므로 리스트 순서를 바꿔도 결과가 같다.
    /// </summary>
    public Boss_PatternSO GetPattern(int id)
    {
        if (patterns == null) return null;

        for (int i = 0; i < patterns.Count; i++)
        {
            if (patterns[i] != null && patterns[i].Id == id) return patterns[i];
        }

        return null;
    }

    // id가 겹치면 쿨다운 딕셔너리 키를 공유해서 한쪽을 쓴 순간 다른 쪽까지 같이 잠긴다.
    // 실제로 pattern_A와 pattern_B가 둘 다 1이어서 근접을 쓰면 원거리까지 막혔다.
    private void Validate_PatternIds()
    {
        if (patterns == null) return;

        for (int i = 0; i < patterns.Count; i++)
        {
            if (patterns[i] == null)
            {
                Debug.LogWarning($"patterns[{i}]가 비어 있다. Boss_Controller의 패턴 리스트에서 빈 칸을 지워야 한다.");
                continue;
            }

            for (int j = i + 1; j < patterns.Count; j++)
            {
                if (patterns[j] == null) continue;
                if (patterns[i].Id != patterns[j].Id) continue;

                Debug.LogWarning($"패턴 id {patterns[i].Id}이 {patterns[i].name}과 {patterns[j].name}에 중복됐다. 한쪽 id를 바꿔야 쿨다운과 상태 연결이 분리된다.");
            }
        }
    }



    /// <summary>
    /// 그로기가 시작된 시점을 기록한다. Boss_GroggyState가 진입하면서 부른다.
    /// </summary>
    public void Begin_Groggy()
    {
        groggyEndTime = Time.time + groggyDuration;
    }

    public void Tick()
    {
        if (boss.FSM.Current == boss.FSM.Groggy || boss.FSM.Current == boss.FSM.Dead)
            OnPattern = false;

        if (boss.FSM.Current == boss.FSM.Groggy)
        {
            if (Time.time < groggyEndTime) return;

            // 밸런스를 되돌리고 나간다.
            boss.Health.Init_Balance();
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }

        // 1. 타겟없음
        if (!context.HasTarget)
        {
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }

        if( (context.BossPosition.x - context.TargetPosition.x) * boss.Moter.Facing > 0)
            boss.Moter.Flip();


        // 패턴 수행중
        if(OnPattern) return;

        if (Time.time < nextDecideTime) return;
        nextDecideTime = Time.time + decideInterval;

        Pattern_Decide();

    }
    private void Pattern_Excution()
    {
        // 주의: OnPattern은 상태 전환에 성공한 뒤에만 켠다. 먼저 켜두면 default로 빠졌을 때
        // 상태는 그대로인데 판단만 영구히 막혀서 보스가 걷기만 하는 상태로 굳는다.
        switch (currentPattern.Id)
        {
            case 1:
                boss.FSM.ChangeState(boss.FSM.A1);
                break;
            default:
                Debug.LogWarning($"패턴 id {currentPattern.Id}({currentPattern.name})에 연결된 상태가 없다. Boss_AI의 switch에 case를 추가해야 실행된다.");
                return;
        }

        OnPattern = true;
        context.Record_PatternUsed(currentPattern.Id);
    }
    /// <summary>
    /// Boss_IdleState, Boss_MoveState, Boss_GroggyState가 자기 Tick에서 부른다.
    /// 공격·사망 상태에서는 호출되지 않으므로, 패턴 도중에 판단이 끼어들 수 없다.
    /// </summary>
    public void Pattern_Decide()
    {
        currentPattern = Select_Pattern();

        // 전부 쿨타임이거나 사거리 밖이다. 걸어 들어가서 다시 판단한다.
        if (currentPattern == null)
        {
            boss.FSM.ChangeState(boss.FSM.Move);
            return;
        }
        Pattern_Excution();
        context.SetPatternID(currentPattern.Id);

        Debug.Log("패턴 ID : "+ currentPattern.Id);
    }

    // 점수를 가중치로 삼는 룰렛이다. 점수가 높을수록 자주 뽑히지만 항상 뽑히지는 않는다.
    // 리스트 인덱스는 이 함수 밖으로 내보내지 않는다. 밖에서 인덱스와 id를 섞어 쓰다가
    // switch가 인덱스를 id로 착각해 pattern_A가 한 번도 실행되지 않았다.
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
