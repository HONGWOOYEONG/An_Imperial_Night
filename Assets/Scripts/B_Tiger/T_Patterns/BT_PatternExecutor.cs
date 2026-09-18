using System.Collections.Generic;
using UnityEngine;

public class BT_PatternExecutor : MonoBehaviour
{
    private B_TigerContext context;
    private readonly Dictionary<string, BT_Pattern> patternRunners = new Dictionary<string, BT_Pattern>();

    private BossPatternData currentPatternData;
    private BT_Pattern currentPatternRunner;
    public bool IsRunning { get; private set; }


    private void Awake()
    {
        context = GetComponent<B_TigerContext>();

        foreach (BT_Pattern patternRunner in GetComponents<BT_Pattern>())
        {
            patternRunners[patternRunner.PatternId] = patternRunner;
        }
    }

    public void Execute(BossPatternData pattern, PlayerContext target)
    {
        if (pattern == null)
        {
            return;
        }

        if (!patternRunners.TryGetValue(pattern.patternId, out BT_Pattern patternRunner))
        {
            return;
        }

        currentPatternData = pattern;
        currentPatternRunner = patternRunner;
        IsRunning = true;

        currentPatternRunner.Begin(this, pattern, target);
    }

    public void Stop()
    {
        currentPatternRunner?.Stop();

        currentPatternRunner = null;
        currentPatternData = null;
        IsRunning = false;
    }

    public void OnPatternAnimationEvent(string eventName)
    {
        if (!IsRunning)
        {
            return;
        }

        currentPatternRunner?.OnAnimationEvent(eventName);
    }

    public void CompleteCurrentPattern(BT_Pattern completedPattern)
    {
        if (!IsRunning || completedPattern != currentPatternRunner)
        {
            return;
        }

        context.UpdateLastPattern(currentPatternData.patternId, Time.time);

        currentPatternRunner = null;
        currentPatternData = null;
        IsRunning = false;
    }
}
