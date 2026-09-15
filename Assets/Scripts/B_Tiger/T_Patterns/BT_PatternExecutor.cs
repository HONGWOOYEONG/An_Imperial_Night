using System.Collections;
using UnityEngine;

public class BT_PatternExecutor : MonoBehaviour
{
    private B_TigerController tigerController;
    
    private Coroutine runningCoroutine;
    private BossPatternData currentPattern;
    public bool IsRunning { get; private set; }

    [SerializeField] private float backWardJumpPower = 5f;
    private float originalGravity;
    private Rigidbody2D rb;
    

    private void Awake()
    {
        tigerController = GetComponent<B_TigerController>();
        originalGravity = tigerController.Rb.gravityScale;
        rb = tigerController.Rb;
    }
    public void Execute(BossPatternData pattern) 
    {
        if(pattern == null) return;
        currentPattern = pattern;

        switch (pattern.patternId)
        {
            case "PatternA":
                runningCoroutine = StartCoroutine(startPatternA());
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

    IEnumerator startPatternA()
    {
        IsRunning = true;
        yield return new WaitForSeconds(1f);

        IsRunning = false;
    }

   
    public void JumpToBackward()
    {
        tigerController.Rb.AddForce(new Vector2(-tigerController.FacingDirection * 1.5f, 0.7f) * backWardJumpPower, ForceMode2D.Impulse);
    }

    public void Freeze()
    {
        tigerController.Rb.gravityScale /= 3;
        tigerController.Rb.linearVelocity /= 6;
    }

    public void IncreaseGravity()
    {
        tigerController.Rb.gravityScale = originalGravity * 2;
    }

    public void ClearStatus()
    {
        tigerController.Rb.gravityScale = originalGravity;
    }

    public void WalkToForeward()
    {
        rb.linearVelocity = new Vector2(tigerController.FacingDirection * 2, rb.linearVelocity.y);
    }

    public void StopWalk()
    {
        tigerController.Stop();
    }

    public void ThrowGhost()
    {
        
    }
}
