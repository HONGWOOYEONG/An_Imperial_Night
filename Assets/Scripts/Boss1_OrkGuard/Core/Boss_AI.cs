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
    private readonly List<Boss_PatternSO> patterns_R;
    private readonly List<Boss_PatternSO> patterns_L;
    private bool isRightHand = true; // 처음엔 오른손
    private bool IsPatternRunning
    {
        get
        {
            return boss.Anim.GetBool("onPattern");
        }
    }

    // 딕셔너리로 왼,오른손 패턴을 묶을까 아니면 따로 할까.
    // {bool, List<Boss_PatternSO>}
    private Boss_PatternSO currentPattern;


    // 판단 사이의 최소 간격. 0이면 공격이 끝나자마자 다음 공격이 붙어서 쉴 틈이 없어진다.
    private readonly float decideInterval;
    private float nextDecideTime = 1f;

    // 그로기가 유지되는 시간. 값은 여기서 들고, 끝났는지 판단도 Decide에서 한다.
    private readonly float groggyDuration;
    private float groggyEndTime;

    // 룰렛을 돌릴 때마다 배열을 새로 잡지 않도록 점수 버퍼를 재사용한다.
    private readonly float[] scores;


    public Boss_AI(Boss_Controller boss, Boss_Context context, List<Boss_PatternSO> patterns_R, List<Boss_PatternSO> patterns_L, float decideInterval, float groggyDuration)
    {
        this.boss = boss;
        this.context = context;
        this.patterns_R = patterns_R;
        this.patterns_L = patterns_L;
        this.decideInterval = decideInterval;
        this.groggyDuration = groggyDuration;

        int countR = patterns_R == null ? 0 : patterns_R.Count;
        int countL = patterns_L == null ? 0 : patterns_L.Count;
        scores = new float[Mathf.Max(countR, countL)];
    }

    public void Begin_Groggy()
    {
        groggyEndTime = Time.time + groggyDuration;
    }

    // 그로기, 사망 시엔 중지. 
    // 그로기는 여기서 타이밍을 계산, 
    // 사망은 controller에서 체크
    public void Tick()
    {
        if (Time.time < groggyEndTime) return;
        if (boss.FSM.Current == boss.FSM.Groggy)
            boss.FSM.ChangeState(boss.FSM.Idle);

        if (!context.HasTarget) {
            boss.FSM.ChangeState(boss.FSM.Idle);
            return;
        }
        if (Time.time < nextDecideTime) return;
        if (IsPatternRunning) return;

        // 결정가능 상태
        // 빈도상 결정 가능 상태에서 부르는게 효율적인걸 알지만 
        // 패턴 결정은 이곳에서 보유시키고 싶다.

        // 혹시모르니 idle,move 조건으로 감싼다.
        if (boss.FSM.Current == boss.FSM.Move && boss.FSM.Current == boss.FSM.Idle)
        {
            if (Pattern_Decide())
                Pattern_Execution();
            else
                ChangeHandJustNow();

        }

    }

    /// <summary>
    /// 패턴이 끝난 시점에 Boss_PatternState가 부른다. 다음 판단을 decideInterval만큼 미뤄
    /// 패턴과 패턴 사이에 여백을 만든다.
    /// ChangeHand상태에 진입해서 바군 handvalue값을 반영하는 애니메이션을 출력한다.
    /// </summary>
    public void Delay_NextDecide()
    {
        nextDecideTime = Time.time + decideInterval;
    }

    /// <summary>
    /// 지금 쓸 패턴을 고르고 상태를 바꾼다. 
    /// 결정 가능 상태 : idle, move
    /// 결정 불가 상태 : pattern, changeHand, groggy, death
    /// </summary>
    public bool Pattern_Decide()
    {
        nextDecideTime = Time.time + decideInterval;

        currentPattern = Select_Pattern(isRightHand);
        return currentPattern;
    }

    // 패턴 성공시에만 호출
    public void Pattern_Execution(){
        // 공격 클래스에 값을 넘김
        boss.Attack.SetPatternToAttack(currentPattern);

        // 쿨다운 기록은 상태를 바꾸기 전에 남긴다. 뒤로 미루면 패턴이 진입 도중 되돌아왔을 때
        // 기록이 통째로 빠지고, 같은 패턴이 다음 판단에서 또 뽑혀 무한히 재진입한다.
        context.Record_PatternUsed(currentPattern.Id);
        context.SetPatternID(currentPattern.Id);
        boss.Anim.SetInteger("patternId",currentPattern.Id);

        // 넘기는 것이 먼저다. ChangeState부터 하면 Enter가 직전 패턴을 한 번 더 재생한다.
        boss.FSM.Pattern.Set_Pattern(currentPattern);
        boss.FSM.ChangeState(boss.FSM.Pattern);
    }

#region 애니메이션 이벤트용 호출 함수.
    // 상태 Enter()
    public void Pattern_Start()
    {
    }
    // 패턴의 종료에 부르는 함수라기 보단 패턴을 종료시키는 것에 가까움.
    public void Pattern_End() 
    {
        ChangeHandJustNow(); // 패턴이 끝난 후 손바꾸기
    }
#endregion


#region 패턴 계산


    private Boss_PatternSO Select_Pattern(bool rightHand)
    {
        var curHand  = rightHand ? patterns_R : patterns_L;
        var selected = Roll_Pattern(curHand);

        return selected;
    }

    /// <summary>
    /// 한 손 리스트 안에서 가중치 룰렛을 돌린다. 후보가 없으면 null.
    /// </summary>
    private Boss_PatternSO Roll_Pattern(List<Boss_PatternSO> set)
    {
        if (set == null || set.Count == 0) return null;

        float totalWeight = 0f;
        for (int i = 0; i < set.Count; i++)
        {
            scores[i] = Evaluate_Pattern(set[i]);
            totalWeight += scores[i];
        }
        if (totalWeight <= 0f) return null;

        float pick = Random.Range(0f, totalWeight);
        for (int i = 0; i < set.Count; i++)
        {
            pick -= scores[i];
            if (pick <= 0f) return set[i];
        }
        return set[set.Count - 1];
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
    
#endregion
#region 손바꾸기

    /// <summary>
    /// 바꿀 손을 확인한다. 그 손에 전환가능한  패턴이 있으면 손을 바꾸고 없으면 대기한다.
    /// SetFloat("rightHand",value);으로 손위치 구분. 오른손=1,왼손=0;
    /// </summary>
    public bool CanIChangeThisHand(bool handChack)
    {
        return Select_Pattern(handChack) != null;
    }

    // 손을 바꾸는 규칙이 아직 혼란. 이에 방법 두 개를 모두 구현
    // 1. 바꿀 수 있을 때 바꿈.
    // 2. 무조건 바꿈. 
    // 우선 원래 계획대로 방법 2를 적용한다.

    public void ChangeHandToPassable()
    {
        if (CanIChangeThisHand(!isRightHand))
        {
            ChangeHandJustNow();
        }
        else
        {
            boss.FSM.ChangeState(boss.FSM.Move);
        }
    }

    private void ChangeHandJustNow()
    {
        isRightHand = !isRightHand;
        float value = isRightHand ? 1f : 0f;
        boss.Anim.SetFloat("rightHand", value);
        boss.FSM.ChangeState(boss.FSM.ChangeHand);

        // 연속 손바꾸기 방지 딜레이. 삭제 가능
        nextDecideTime = Time.time + decideInterval;
    }
#endregion
}
