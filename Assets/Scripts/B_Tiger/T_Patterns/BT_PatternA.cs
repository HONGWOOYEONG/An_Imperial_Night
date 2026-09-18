using System.Collections;
using UnityEngine;

public class BT_PatternA : BT_Pattern
{
    private Coroutine runningCoroutine;

    public override string PatternId => "PatternA";

    public override void Begin(BT_PatternExecutor patternExecutor, BossPatternData patternData)
    {
        base.Begin(patternExecutor, patternData);
        runningCoroutine = StartCoroutine(RunPattern());
    }

    public override void OnAnimationEvent(string eventName)
    {
        
    }

    public override void Stop()
    {
        if (runningCoroutine == null)
        {
            return;
        }

        StopCoroutine(runningCoroutine);
        runningCoroutine = null;
    }

    private IEnumerator RunPattern()
    {
        yield return new WaitForSeconds(1f);
        runningCoroutine = null;
        Complete();
    }
}
