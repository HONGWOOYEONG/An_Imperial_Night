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
    }

    private void Update()
    {
        currentPosition = transform.position;
    }

    // 피격 시 호출하기
    public void updateStatus(float hp, float posture)
    {
        currentHP = hp;
        currentPosture = posture;
    }

    public void updatePhase(float phase) { currentPhase = phase; }
    public void updateLastPattern(string patternID , float lastUsedTime) 
    { 
        patternDict[patternID] = lastUsedTime;
    }

    public float GetPattern(string patternID)
    {
        return patternDict[patternID];
    }

    public Vector3 getCurrentPosition() { return  currentPosition; }

}
