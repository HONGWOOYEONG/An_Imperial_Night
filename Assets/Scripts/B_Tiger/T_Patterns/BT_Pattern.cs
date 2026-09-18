using UnityEngine;

public abstract class BT_Pattern : MonoBehaviour
{
    protected BT_PatternExecutor executor;

    public abstract string PatternId { get; }

    public virtual void Begin(BT_PatternExecutor patternExecutor, BossPatternData patternData, PlayerContext target)
    {
        executor = patternExecutor;
    }

    public abstract void OnAnimationEvent(string eventName);

    public virtual void Stop()
    {
        
    }

    protected void Complete()
    {
        executor?.CompleteCurrentPattern(this);
    }
}
