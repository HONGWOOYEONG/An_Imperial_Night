using UnityEngine;
using static UnityEditor.PlayerSettings;

//거미줄 뿌리기
public class P_PatternF_State : IPuppeteerState
{
    private Vector2 myPos;
    private Vector2 maxLeftPos; //왼쪽 좌표 최대값
    private Vector2 maxRightPos; //오른쪽 좌표 최대값
    private float randLeftX;
    private float randRightX;
    private float addY = 2f;
   
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("F 상태 시작");
        
          myPos = new Vector2(controller.transform.position.x, controller.transform.position.y + addY);

            maxLeftPos = new Vector2(controller.transform.position.x - controller.enemyRange, controller.transform.position.y);
            randLeftX = Random.Range(maxLeftPos.x, myPos.x);
            controller.leftSpiderWeb = Object.Instantiate(controller.spiderWeb, controller.throwFire.position, Quaternion.identity);
            Debug.Log("왼쪽 거미줄 생성");
            SpiderWeb leftSpiderWeb = controller.leftSpiderWeb.GetComponent<SpiderWeb>();
            if (leftSpiderWeb != null)
            {
               leftSpiderWeb.endPos = new Vector2(randLeftX, controller.transform.position.y);
            }

           
            maxRightPos = new Vector2(controller.transform.position.x + controller.enemyRange, controller.transform.position.y);
            randRightX = Random.Range(myPos.x, maxRightPos.x);
            controller.rightSpiderWeb = Object.Instantiate(controller.spiderWeb, controller.throwFire.position, Quaternion.identity);
            Debug.Log("오른쪽 거미줄 생성");
            SpiderWeb rightSpiderWeb = controller.rightSpiderWeb.GetComponent<SpiderWeb>();
            if(rightSpiderWeb != null)
            {
                rightSpiderWeb.endPos = new Vector2(randRightX, controller.transform.position.y);
            }

    }

    public void Exit(E_PuppeteerController controller)
    {
      myPos = Vector2.zero;
      maxLeftPos = Vector2.zero; //왼쪽 좌표 최대값
      maxRightPos = Vector2.zero; //오른쪽 좌표 최대값
      randLeftX = 0;
      randRightX = 0;
      controller.leftSpiderWeb = null;
      controller.rightSpiderWeb = null;
      Debug.Log("F 상태 종료");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (controller.isPatternEnded_F)
        {
            controller.isPatternEnded_F = false;
            controller.ChangeState(controller.states["idle"]);
        }
    }

  
}
