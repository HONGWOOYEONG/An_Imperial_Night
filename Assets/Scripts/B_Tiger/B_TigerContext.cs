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

        return 0f;
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
