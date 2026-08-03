using UnityEngine;

//현재 
public class Enemy_MoveBox : MonoBehaviour
{
    E_PuppeteerController e_Puppeteer;

    private void Start()
    {
        e_Puppeteer = GetComponentInParent<E_PuppeteerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (e_Puppeteer == null || e_Puppeteer.targetPlayer == null)
        {
            return;
        }
        if (collision.CompareTag (e_Puppeteer.targetPlayer.tag)) //현재 타겟 플레이어가 히트박스 안에 들어온 플레이어와 같다면
        {
            e_Puppeteer.isInTargetPlayer = true;
            Debug.Log("플레이어 진입 감지");
        }
     
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (e_Puppeteer == null || e_Puppeteer.targetPlayer == null)
        {
            return;
        }
        if (collision.CompareTag(e_Puppeteer.targetPlayer.tag))
        {
            e_Puppeteer.isInTargetPlayer = false;
        }
    }
}
