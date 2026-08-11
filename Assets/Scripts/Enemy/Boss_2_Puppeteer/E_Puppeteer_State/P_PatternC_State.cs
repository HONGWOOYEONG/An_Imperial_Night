using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;


//말처럼 달리기
public class P_PatternC_State : IPuppeteerState
{
    float BASE_FPS = 60f;
    private Rigidbody2D rb;
    private Collider2D nearTarget;
    private Collider2D farTarget;
    private float range_RandP = 4f; //태자랑 도아경의 거리(멀리있는지 알기 위한 임의의 수)


    [Header("UpperSwing")]
    private float frontDelay_U = 5f;
    private float backDelay_U = 5f;
    private float speed_upper = 7f;

    [Header("JumpSlam")]
    private float frontDelay_J = 5f;
    private float backDelay_J = 5f;
    private float jumpDuration = 1f; //임의
    private float jumpForce = 4f; //임의

    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("PatternC 상태 시작");

        rb = controller.GetComponent<Rigidbody2D>();

        if (controller.targetPlayer != null)
        {
            if(controller.targetPlayer == controller.rangedDealer)
            {
                controller.isRaged = true;
            }
        }

        Vector2 myPos = controller.transform.position;
        Vector2 rangedPos = new Vector2(controller.rangedDealer.transform.position.x, myPos.y);
        Vector2 meleePos = new Vector2(controller.meleeDealer.transform.position.x, myPos.y);
        float distance_RandP = Vector2.Distance(rangedPos, myPos);
        float distance_MandP =Vector2.Distance(meleePos, myPos);

        nearTarget = distance_MandP > distance_RandP ? controller.rangedDealer : controller.meleeDealer;
        farTarget = nearTarget == controller.meleeDealer ? controller.rangedDealer : controller.meleeDealer;

        controller.StartCoroutine(StartComboAttack(controller));
    }

    public void Exit(E_PuppeteerController controller)
    {
        controller.isCase_1 = false;
        controller.isRaged = false;
        controller.countAtk = 0;
        Debug.Log("PatternC 상태 종료");
    }
    
    public void Update(E_PuppeteerController controller)
    {
       if(controller.targetPlayer == null) { return; }
       if(controller.countAtk == 0)
        {
            controller.ChangeState(controller.idle);
        }        
    }
    IEnumerator StartComboAttack(E_PuppeteerController controller)
    {
        if(controller.targetPlayer == null) { yield break; }

        int count = 0; 
        while(count < controller.countAtk)
        {
            if (count == 0) controller.targetPlayer = nearTarget;
            else if (count == 1) controller.targetPlayer = farTarget;
            else if (count == 2) controller.targetPlayer = controller.rangedDealer;
            
            if(controller.targetPlayer == null) { yield break; }


            while (!controller.isInTargetPlayer)
            {
                Debug.Log("이동 중");
                Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
                controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos, Time.deltaTime * speed_upper);

                yield return null;
            }

                if(count == 0)
                {
                    yield return controller.StartCoroutine(UpperSwing(controller));
                }
                else if(count == 1)
                {
                    
                    yield return controller.StartCoroutine(JumpSlam(controller));
                }
                else //3타
                {                   
                    if (controller.isRaged)
                    {
                        //태자 전하와 도아경이 멀리있을 때 가까이 있을 때
                        Vector2 myPos = controller.transform.position;
                        Vector2 rangedPos = new Vector2(controller.rangedDealer.transform.position.x, myPos.y);
                        float distance_RandP = Vector2.Distance(myPos, rangedPos);

                        if(range_RandP < distance_RandP) //태자전하가 도아경과 멀리 있다면
                        {
                            yield return controller.StartCoroutine(UpperSwing(controller));
                        }
                        else //가까이 있다면
                        {
                            yield return controller.StartCoroutine(JumpSlam(controller));

                        }
                    }
                    else // controller.isRanged = false
                    {
                       yield return controller.StartCoroutine(JumpSlam(controller));
                    }
                }
                count++;
        }
        controller.ChangeState(controller.idle);
    }


    //올려치기
    IEnumerator UpperSwing(E_PuppeteerController controller)
    {
        if(controller.targetPlayer == null) { yield break ; }
        yield return new WaitForSeconds(frontDelay_U/BASE_FPS); //선딜


        yield return new WaitForSeconds(backDelay_U/BASE_FPS); //후딜
    }
   
    //점프하여 내려찍기
    IEnumerator JumpSlam(E_PuppeteerController controller)
    {
        if (controller.targetPlayer == null) { yield break; }
   
        float timer = 0f;
        Vector2 startPos = controller.transform.position;
        Vector2 rangedPos = new Vector2(controller.rangedDealer.transform.position.x, startPos.y);

        if (rb != null) rb.linearVelocity = Vector2.zero;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;

            Vector2 currentPos = Vector2.Lerp(startPos, rangedPos, t); 

            currentPos.y += 4 * jumpForce * t * (1 - t); //포물선 점프 공식

            controller.transform.position = currentPos;
            yield return null;

        }
        controller.transform.position = rangedPos; 

        yield return new WaitForSeconds(frontDelay_J / BASE_FPS); //선딜
        yield return new WaitForSeconds(backDelay_J / BASE_FPS); //후딜
    }
}
