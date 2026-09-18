using System.Collections.Generic;
using UnityEngine;

public class B_TigerUtilityAI : MonoBehaviour
{
    [Header("Pattern List")]
    [SerializeField] private List<BossPatternData> patterns;

    private B_TigerContext bTigerContext;

    [Header("Playable Character")]
    [SerializeField] private PlayerContext hPlayerContext;
    [SerializeField] private PlayerContext tPlayerContext;

    private PlayerContext currentTarget;

    private void Awake()
    {
        bTigerContext = GetComponent<B_TigerContext>();
    }

    public PlayerContext GetCurrentTarget()
    {
        return currentTarget;
    }

    private PlayerContext GetTarget(BossPatternData pattern)
    {
        return pattern.target switch
        {
            "H" => hPlayerContext,
            "T" => tPlayerContext,
            "Nearest" => GetNearestTarget(),
            "Farthest" => GetFarthestTarget(),
            _ => null
        };
    }

    public BossPatternData SelectPattern()
    {
        if (bTigerContext == null || patterns == null || patterns.Count == 0)
        {
            currentTarget = null;
            return null;
        }

        List<float> patternScores = new List<float>();
        float totalWeight = 0f;

        foreach (BossPatternData pattern in patterns)
        {
            float score = EvaluatePattern(pattern);
            patternScores.Add(score);
            totalWeight += score;
        }

        if (totalWeight <= 0f)
        {
            currentTarget = null;
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);

        for (int i = 0; i < patterns.Count; i++)
        {
            randomValue -= patternScores[i];

            if (randomValue <= 0f)
            {
                BossPatternData selectedPattern = patterns[i];
                currentTarget = GetTarget(selectedPattern);
                return selectedPattern;
            }
        }

        currentTarget = null;
        return null;
    }

    private float EvaluatePattern(BossPatternData pattern)
    {
        if (pattern == null)
        {
            return 0f;
        }

        if (IsOnCooldown(pattern))
        {
            return 0f;
        }

        PlayerContext target = GetTarget(pattern);
        if (target == null)
        {
            return 0f;
        }

        float currentDistance = GetTargetDistance(target);
        if (currentDistance < pattern.minDistance || currentDistance > pattern.maxDistance)
        {
            return 0f;
        }

        float distanceScore = CalculateDistanceScore(currentDistance, pattern);
        return pattern.baseWeight * distanceScore;
    }

    private bool IsOnCooldown(BossPatternData pattern)
    {
        float lastUsedTime = bTigerContext.GetLastPatternTime(pattern.patternId);
        return lastUsedTime + pattern.skillCooldown >= Time.time;
    }

    private float GetTargetDistance(PlayerContext targetContext)
    {
        return Vector3.Distance(bTigerContext.GetCurrentPosition(), targetContext.getPosition());
    }

    private float CalculateDistanceScore(float currentDistance, BossPatternData pattern)
    {
        float distanceFromPreferred = Mathf.Abs(currentDistance - pattern.preferredDistance);
        float range = pattern.maxDistance - pattern.minDistance;

        if (range <= 0f)
        {
            return 0f;
        }

        return 1f - (distanceFromPreferred / range);
    }

    private PlayerContext GetNearestTarget()
    {
        if (hPlayerContext == null) return tPlayerContext;
        if (tPlayerContext == null) return hPlayerContext;

        float hDistance =(hPlayerContext.getPosition() - (Vector2)transform.position).sqrMagnitude;

        float tDistance =(tPlayerContext.getPosition() - (Vector2)transform.position).sqrMagnitude;

        return hDistance <= tDistance ? hPlayerContext : tPlayerContext;
    }

    private PlayerContext GetFarthestTarget()
    {
        if (hPlayerContext == null) return tPlayerContext;
        if (tPlayerContext == null) return hPlayerContext;

        float hDistance = (hPlayerContext.getPosition() - (Vector2)transform.position).sqrMagnitude;

        float tDistance = (tPlayerContext.getPosition() - (Vector2)transform.position).sqrMagnitude;

        return hDistance >= tDistance ? hPlayerContext : tPlayerContext;
    }

    public PlayerContext GetTraceTarget()
    {
        return GetNearestTarget();
    }

}
