/// 작성자 : 유희일
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "지금 어떤 상황인가"를 묻는 창구. 값을 바꾸는 것은 각 소유자가 하고 여기는 모아서 답만 한다.
/// MonoBehaviour가 아니다. Boss_Controller가 생성해서 들고 있는다.
///
/// 여기 두지 않는 것
/// - 판단 : 어떤 패턴을 고를지는 Boss_AI가 이 값들을 보고 정한다.
/// - 체력 수치 : Boss_Health가 소유한다. 필요하면 Controller를 통해 묻는다.
/// </summary>
public class Boss_Context
{
    private const float NoTargetDistance = float.MaxValue;
    public const int NoPattern = -1;
    private readonly Transform bossTransform;
    private readonly Dictionary<string, float> patternLastUsedTime = new Dictionary<string, float>();

    // 플레이어는 1~2명이다. 한 명만 들고 판단하면 2인 플레이에서
    // 뒤에서 때리는 쪽을 영원히 무시하게 된다.
    private readonly PlayerContext[] targets = new PlayerContext[2];

    public int NextPatternIndex { get; private set; } = NoPattern;
    public int Phase { get; private set; }

    public Vector2 BossPosition => bossTransform.position;

    public Boss_Context(Transform bossTransform)
    {
        this.bossTransform = bossTransform;
    }

    /// <summary>
    /// 지금 보스가 기준으로 삼는 플레이어.
    /// 임시로 거리기준으로 측정했다.
    /// </summary>
    public PlayerContext CurrentTarget
    {
        get
        {
            PlayerContext nearest = null;
            float nearestDistance = NoTargetDistance;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;

                float distance = Vector2.Distance(BossPosition, targets[i].getPosition());
                if (distance >= nearestDistance) continue;

                nearest = targets[i];
                nearestDistance = distance;
            }

            return nearest;
        }
    }

    public bool HasTarget => CurrentTarget != null;

    public Vector2 TargetPosition
    {
        get
        {
            PlayerContext target = CurrentTarget;
            return !HasTarget
                ? BossPosition 
                : (Vector2)target.getPosition();
        }
    }

    public float DistanceToTarget
    {
        get
        {
            PlayerContext target = CurrentTarget;
            return !HasTarget 
                ? NoTargetDistance 
                : Vector2.Distance(BossPosition, target.getPosition());
        }
    }


#region 세팅 함수
    /// <summary>
    /// 1인 플레이면 second에 null을 넘긴다.
    /// </summary>
    public void SetTargets(PlayerContext first, PlayerContext second)
    {
        targets[0] = first;
        targets[1] = second;
    }

    public void SetPhase(int phase)
    {
        Phase = phase;
    }

    public void SetNextPattern(int patternIndex)
    {
        NextPatternIndex = patternIndex;
    }

    /// <summary>
    /// 패턴을 실제로 시작한 시점에 호출
    /// </summary>
    public void Record_PatternUsed(string patternId)
    {
        if (string.IsNullOrEmpty(patternId)) return;

        patternLastUsedTime[patternId] = Time.time;
    }

    // 수치는 패턴 데이터를 들고 있는 Boss_AI가 넘긴다. 여기는 기록만 한다.
    public bool IsOnCooldown(string patternId, float cooldown)
    {
        if (string.IsNullOrEmpty(patternId)) return false;

        if (!patternLastUsedTime.TryGetValue(patternId, out float lastUsedTime)) return false;

        return Time.time < lastUsedTime + cooldown;
    }
#endregion
}
