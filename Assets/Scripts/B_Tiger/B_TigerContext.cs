using System.Collections.Generic;
using UnityEngine;

public class B_TigerContext : MonoBehaviour
{
    private float currentHP;
    private float currentPosture;
    private float currentPhase;

    private Transform thisTransform;
    private Vector3 currentPosition;

    private Dictionary<string, float> patternDict;
    
    private void Awake()
    {
        thisTransform = GetComponent<Transform>();
        patternDict = new Dictionary<string, float>();
    }

    private void Update()
    {
        currentPosition = thisTransform.position;
    }

    public void UpdateStatus(float hp, float posture)
    {
        currentHP = hp;
        currentPosture = posture;
    }

    public void UpdatePhase(float phase)
    {
        currentPhase = phase;
    }

    public void UpdateLastPattern(string patternID, float lastUsedTime)
    {
        if (string.IsNullOrEmpty(patternID))
        {
            return;
        }

        patternDict[patternID] = lastUsedTime;
    }

    public float GetLastPatternTime(string patternID)
    {
        if (string.IsNullOrEmpty(patternID))
        {
            return 0f;
        }

        if (patternDict.TryGetValue(patternID, out float value))
        {
            return value;
        }

        // 사용 기록이 없는 패턴은 게임 시작 직후에도 선택될 수 있어야 합니다.
        // 음의 무한대를 반환하면 lastUsedTime + cooldown 검사가 항상 false가 됩니다.
        return float.NegativeInfinity;
    }

    public float GetHP()
    {
        return currentHP;
    }

    public float GetPosture()
    {
        return currentPosture;
    }

    public float GetPhase()
    {
        return currentPhase;
    }

    public Vector3 GetCurrentPosition()
    {
        return currentPosition;
    }
}
