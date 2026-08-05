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
        CheckTargetCollision(collision, true);
        Debug.Log("플레이어 진입");
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        CheckTargetCollision(collision, true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        CheckTargetCollision(collision, false);
    }

    private void CheckTargetCollision(Collider2D collision, bool isInArea)
    {
        if (e_Puppeteer == null || e_Puppeteer.targetPlayer == null)
        {
            return;
        }

        GameObject target = e_Puppeteer.targetPlayer.gameObject;
        if(collision.gameObject == target)
        {
            e_Puppeteer.isInTargetPlayer = isInArea;
        }
    }

}
