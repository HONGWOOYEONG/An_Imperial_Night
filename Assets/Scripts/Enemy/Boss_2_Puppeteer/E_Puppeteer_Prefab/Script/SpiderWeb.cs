using System.Threading;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private E_PuppeteerController controller;
    private Vector2 instantiatePos;
    private Vector2 startPos; //시작
    [SerializeField] public Vector2 endPos; //끝
    private float addY = 1f;
    private float moveSpeed = 16f;
    void Start()
    {
        controller = GameObject.FindWithTag("Enemy_Puppeteer").GetComponent<E_PuppeteerController>();
    }

    void Update()
    {

        if (startPos != null && endPos != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, endPos, moveSpeed * Time.deltaTime);
        }
    }
    private void OnDestroy()
    {
        controller.isThrowSpiderWeb = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (controller.currentState is P_PatternF_State || controller.currentState is P_PatternC_State)
        {
            if (other.gameObject.tag == "Ground")
            {
                Debug.Log("땅 명중");
                instantiatePos = new Vector2(endPos.x, endPos.y + addY);
                Instantiate(controller.spiderwebSwamp, instantiatePos, Quaternion.identity);
                Debug.Log("거미줄 늪 생성");
                Destroy(gameObject);
            }
            else if (other.gameObject.tag == "RangedDealer" || other.gameObject.tag == "MeleeDealer")
            {
                Debug.Log( other.tag + " 명중 , 포박");

                //포박 상태 이동을 못하게만들고 거미줄 덩어리의 도착 지점까지 넉백을 시킴
                playerMovement = other.gameObject.GetComponent<PlayerMovement>();
                if(playerMovement != null)
                {
                    playerMovement.isMoving = false; //포박상태
                }
             

                Vector2 myPos = controller.transform.position;
                Vector2 targetPos = other.transform.position;
                float dirX = (targetPos.x - myPos.x) > 0 ? 1 : -1;
                Vector2 dirToTarget = new Vector2(dirX, 0f);
                playerMovement.KnockBack(dirToTarget); //넉백
                playerMovement.isMoving = true; //넉백이 다 된 후 다시 움직일 수 있게 해줌
                Debug.Log(playerMovement.isMoving);
               
                instantiatePos = new Vector2(endPos.x, endPos.y + addY);
                Instantiate(controller.spiderwebSwamp, instantiatePos, Quaternion.identity);
                Debug.Log("거미줄 늪 생성");
                Destroy(gameObject);
            }
        }
       
    }
}
