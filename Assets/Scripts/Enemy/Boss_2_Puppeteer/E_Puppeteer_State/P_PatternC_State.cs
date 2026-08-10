using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

//말처럼 달리기
public class P_PatternC_State : IPuppeteerState
{
    float BASE_FPS = 60f;
    string[] target;

    [Header("UpperSwing")]
    private float frontDelay_U = 5f;
    private float backDelay_U = 5f;
    private float speed_upper = 7f;

    [Header("DownSlam")]
    private float frontDelay_D = 5f;
    private float backDelay_D = 5f;

    [Header("JumpSlam")]
    private float frontDelay_J = 5f;
    private float backDelay_J = 5f;


    //만약 case1이 true이고 
    //만약 대상이 태자일 때
    //만약 대상이 회월일 때
    public void Enter(E_PuppeteerController controller)
    {
        
        Debug.Log("PatternC 상태 시작");
        if(controller.isCase_1 == true)
        {
            if(controller.targetPlayer == controller.meleeDealer) //회월 태자 태자
            {
                target = new string[] {"회월","태자", "태자"};
              
            }
            else if(controller.targetPlayer == controller.rangedDealer) //태자 회월 태자
            {
                target = new string[] { "태자", " 회월", "태자" };
            }
        }
        else
        {
            //가장 가까운 플레이어에게 창을 위로 올려친다.
            controller.StartCoroutine(UpperSwing(controller));
        }
    }

    public void Exit(E_PuppeteerController controller)
    {
        controller.isCase_1 = false;
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
    void move()
    { 
        
    }

    //올려치기
    IEnumerator UpperSwing(E_PuppeteerController controller)
    {
        if(controller.targetPlayer == null) { yield break ; }
        yield return new WaitForSeconds(frontDelay_U/BASE_FPS); //선딜

        Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
        controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos,Time.deltaTime * speed_upper);

        yield return new WaitForSeconds(backDelay_U/BASE_FPS); //후딜
    }
    //내려 찍기
    IEnumerator DownSlam(E_PuppeteerController controller) 
    {
        if (controller.targetPlayer == null) { yield break; }
        yield return new WaitForSeconds(frontDelay_D / BASE_FPS); //선딜

        Vector2 targetPos = new Vector2(controller.targetPlayer.transform.position.x, controller.transform.position.y);
        controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos, Time.deltaTime * speed_upper);

        yield return new WaitForSeconds(backDelay_D / BASE_FPS); //후딜
    }
    //점프하여 내려찍기
    IEnumerator JumpSlam(E_PuppeteerController controller)
    {
        if (controller.targetPlayer == null) { yield break; }
        Vector2 myPos = controller.transform.position;
        Vector2 rangedPos = new Vector2(controller.rangedDealer.transform.position.x, myPos.y);
        Vector2 meleePos = new Vector2(controller.meleeDealer.transform.position.x, myPos.y);

        yield return new WaitForSeconds(frontDelay_J / BASE_FPS); //선딜
        yield return new WaitForSeconds(backDelay_J / BASE_FPS); //후딜
    }
}
