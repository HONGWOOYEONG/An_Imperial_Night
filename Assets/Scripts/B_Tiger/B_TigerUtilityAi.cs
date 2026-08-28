using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class B_TigerUtilityAI : MonoBehaviour
{
    [Header("Pattern List")]
    [SerializeField] private List<BossPatternData> patterns;

    private B_TigerContext B_TigerContext;

    [Header("Playable Character")]
    [SerializeField]private PlayerContext H_playerContext;
    [SerializeField]private PlayerContext T_playerContext;

    private void Awake()
    {
        B_TigerContext = GetComponent<B_TigerContext>();
    }

    public BossPatternData SelectPattern()
    {
        List<float> patternsScores = new List<float>();
        float totalWeight = 0f;

        foreach (BossPatternData pattern in patterns)
        {
            float score = EvaluatePattern(pattern);
            
            patternsScores.Add(score);
            totalWeight += score;
        }

        if (totalWeight <= 0f) return null; // 추후 쿨타임시 작동하는 패턴으로 설정

        float randomValue = Random.Range(0f, totalWeight);

        for (int i = 0; i < patterns.Count; i++)
        {
            randomValue -= patternsScores[i];

            if (randomValue <= 0f) return patterns[i];
        }

        return null;

    }

    private float EvaluatePattern(BossPatternData pattern)
    {
        if (IsOnCooldown(pattern)) return 0;

        float currentDistance = GetTargetDistance();

        if (currentDistance < pattern.minDistance ||currentDistance > pattern.maxDistance)
        {
            return 0f;
        }

        float distanceScore = CalculateDistanceScore(currentDistance,pattern);
        float finalScore = pattern.baseWeight * distanceScore;

        return finalScore;
    }

    private bool IsOnCooldown(BossPatternData pattern)
    {
        if(pattern == null) return false;
        if (B_TigerContext.GetPattern(pattern.patternId) + pattern.skillCooldown < Time.time) return false;
        // lastUsed랑 쿨타임 더해서 Time.time보다 작으면 false
        return true;
    }

    private float GetTargetDistance()
    {
         
        return Vector3.Distance(B_TigerContext.getCurrentPosition(), H_playerContext.getPosition());
    }

    private float CalculateDistanceScore(float currentDistance,BossPatternData pattern)
    {
        float distanceFromPreferred = Mathf.Abs(currentDistance - pattern.preferredDistance);
        float range = pattern.maxDistance - pattern.minDistance;
        
        return 1f - (distanceFromPreferred / range);
    }

}