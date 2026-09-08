using System.Collections;
using UnityEngine;


//점프 공격→몸통 돌리기→패턴 a 5타
public class P_PatternG_State : IPuppeteerState
{
    private E_PuppeteerAction action = new E_PuppeteerAction();

    private Rigidbody2D rb;
    
    private float moveSpeed = 8f;

    [Header("JumpAttack")]
    private float frontDelayJump = 3f;//임의
    private float backDelayJump = 3f; //임의
    private Vector2 targetPos;
    private float jumpDuration = 1f; //임의
    private float jumpForce = 6f; //임의

    [Header("SpinBody")]
    private bool isSpining = true;
    private float frontDelaySpin = 3f;//임의
    private float backDelaySpin = 3f; //임의

    [Header("NormalAttack")]
    private float frontDelayNormal = 5;//임의
    private float backDelayNormal = 10; //임의
    private float attackHoldTime = 2f; //임의(판정 지속 시간)

    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("패턴 G 시작");
        if (controller.rb != null)
        {
            rb = controller.rb;
        }

        controller.StartCoroutine(StartAttack(controller));
     
    }

    public void Exit(E_PuppeteerController controller)
    {
        Debug.Log("패턴 G 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (controller.targetPlayer == null) { return; }

        if (!isSpining)
        {
            controller.LookAtLocation(controller.targetPlayer.transform.position.x); //flip
        }
    }

    private IEnumerator StartAttack(E_PuppeteerController controller)
    {
        //점프 공격
        yield return new WaitForSeconds(frontDelayJump / controller.BASE_FPS);
        Vector2 startPos = controller.transform.position; //점프 시작 위치
        targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y); //점프 끝 위치
        yield return action.JumpAttack(controller, rb, jumpDuration, startPos, targetPos, jumpForce);
        yield return new WaitForSeconds(backDelayJump / controller.BASE_FPS);

        action.SetNearTargetPlayer(controller);
        yield return controller.StartCoroutine(MoveTarget(controller));

        //몸통 돌리기 공격
        yield return new WaitForSeconds(frontDelaySpin / controller.BASE_FPS);
        action.SpinBody(controller);
        yield return new WaitForSeconds(backDelaySpin / controller.BASE_FPS);

        action.SetNearTargetPlayer(controller);
        yield return controller.StartCoroutine(MoveTarget(controller));

        //일반 공격
        yield return new WaitForSeconds(frontDelayNormal / controller.BASE_FPS);
        yield return action.NormalAttack(controller, attackHoldTime);
        yield return new WaitForSeconds(backDelayNormal / controller.BASE_FPS);

        controller.ChangeState(controller.states["idle"]);
    }
   
    private IEnumerator MoveTarget(E_PuppeteerController controller)
    {
        Debug.Log("이동 중");
        while (!controller.isInTargetPlayer)
        {
            if (controller.targetPlayer == null) break;

            Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
            controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos, moveSpeed * Time.deltaTime);

            yield return null;
        }
    }

   
}
