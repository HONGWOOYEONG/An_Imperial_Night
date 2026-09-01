using System.Collections;
using UnityEngine;


public class E_PuppeteerAction : MonoBehaviour
{

    [Header("패턴 C")]
    Vector2 targetPos_C;
    public bool isThrowSpiderWeb = true;
    private float addButtPosX = 5f;
    private float speed_Butt = 10f;

    #region 패턴 A
    public void Rush(E_PuppeteerController controller, Vector2 targetPos, float rushSpeed)
    {
        controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos, rushSpeed * Time.deltaTime);
    }

    public IEnumerator Attack(E_PuppeteerController controller, float[] attackHoldTime)
    {
        controller.isAttaking_A = true;
        controller.HitBox_A.SetActive(true); //히트박스 킴
        yield return new WaitForSeconds(attackHoldTime[controller.currentCount] / controller.BASE_FPS);
        controller.HitBox_A.SetActive(false); //히트박스 끔
        controller.isAttaking_A = false;
    }


    public void ChangeTarget(E_PuppeteerController controller)
    {
        Vector2 rangedD = new Vector2(controller.rangedDealer.transform.position.x, controller.transform.position.y);
        Vector2 meleeD = new Vector2(controller.meleeDealer.transform.position.x, controller.transform.position.y);
        float rangedDis = Vector2.Distance(controller.transform.position, rangedD); //적과 원거리 거리 계산
        float meleeDis = Vector2.Distance(controller.transform.position, meleeD); //적과 근거리 거리 계산

        controller.targetPlayer = rangedDis > meleeDis ? controller.meleeDealer : controller.rangedDealer;
    }
    #endregion

    #region 패턴 C
    public IEnumerator HeavyLariat(E_PuppeteerController controller, float frontDelay_Lariat, float backDelay_Lariat) //헤비레리어트 
    {
        Debug.Log("해비 레리어트 시작");
        yield return new WaitForSeconds(frontDelay_Lariat / controller.BASE_FPS);

        //.SetAcitve("true");
        //헤이레이어트 공격

        yield return new WaitForSeconds(backDelay_Lariat / controller.BASE_FPS);
        Debug.Log("해비 레리어트 종료");
    }

    public IEnumerator HeadButt(E_PuppeteerController controller, float frontDelay_Head, float backDelay_Head) //머리박치기
    {
        Debug.Log("머리박치기 시작");
        yield return new WaitForSeconds(frontDelay_Head / controller.BASE_FPS);

        //현재 플레이어 x값과 적의 x값을 비교해서 오른쪽에 있으면 + 왼쪽에 있으면 -
        float myX = controller.transform.position.x;    
        float targetX = controller.targetPlayer.transform.position.x;

        if (myX > targetX) // 적이 오른쪽에 있다면
        {
            targetPos_C = new Vector2(controller.transform.position.x - addButtPosX, controller.transform.position.y);
        }
        else //적이 왼쪽에 있다면
        {
            targetPos_C = new Vector2(controller.transform.position.x + addButtPosX, controller.transform.position.y);
        }
            
        // 일정 시간 동안 돌진
        float elapsed = 0f;
        while (elapsed < 0.3f) // 0.3초 동안 돌진
        {
            controller.transform.position = Vector2.MoveTowards(controller.transform.position, targetPos_C, speed_Butt * Time.deltaTime );
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(backDelay_Head / controller.BASE_FPS);
        Debug.Log("머리박치기 종료");
    }

    public void CreateSpiderWeb(GameObject spiderWeb,Vector2 firePos ,Vector2 targetPos)
    {
        
        GameObject web = Instantiate(spiderWeb, firePos, Quaternion.identity);
        SpiderWeb spider = web.GetComponent<SpiderWeb>();
        spider.endPos = targetPos;

        isThrowSpiderWeb = false;

    }
    #endregion 
}
