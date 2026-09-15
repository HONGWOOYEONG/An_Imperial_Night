using UnityEngine;

public class BT_PatternExecutor : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private B_TigerController tigerController;

    private Coroutine runningCoroutine;
    private BossPatternData currentPattern;
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        if(tigerController == null) 
        {
            tigerController = GetComponent<B_TigerController>();
        }

    }
    public void Execute(BossPatternData pattern) 
    {
        if(pattern == null) return;

        switch (pattern.patternId)
        {
            case "PatternA":
                break;
            case "PatternB":
                break;
            case "PatternC":
                break;
            case "PatternD":
                break;
            case "PatternE":
                break;
        }
    }

    public void Stop()
    {

    }
}
