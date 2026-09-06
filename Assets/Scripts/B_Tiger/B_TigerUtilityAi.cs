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

    private void Awake()
    {
        bTigerContext = GetComponent<B_TigerContext>();
    }

    public void SetTarget(PlayerContext playerContext)
    {
        hPlayerContext = playerContext;
    }

    public BossPatternData SelectPattern()
    {
        if (bTigerContext == null || hPlayerContext == null)
        {
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
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);

        for (int i = 0; i < patterns.Count; i++)
        {
            randomValue -= patternScores[i];

            if (randomValue <= 0f)
            {
                return patterns[i];
            }
        }

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

        float currentDistance = GetTargetDistance();
        if (currentDistance < pattern.minDistance || currentDistance > pattern.maxDistance)
        {
            return 0f;
        }

        float distanceScore = CalculateDistanceScore(currentDistance, pattern);
        return pattern.baseWeight * distanceScore;
    }

    private bool IsOnCooldown(BossPatternData pattern)
    {
        float lastUsedTime = bTigerContext.GetPattern(pattern.patternId);
        return lastUsedTime + pattern.skillCooldown >= Time.time;
    }

    private float GetTargetDistance()
    {
        return Vector3.Distance(bTigerContext.GetCurrentPosition(), hPlayerContext.getPosition());
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
}
