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

                playerMovement = other.GetComponentInParent<PlayerMovement>();
                if (playerMovement == null) return;

                PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
                if (playerHealth == null) return;
                playerHealth.ReceiveAttack(new DamageInfo { damageType = DamageType.Bind });
                if (playerHealth.IsBound) playerMovement.KnockBackToPoint(endPos, moveSpeed);

               
                instantiatePos = new Vector2(endPos.x, endPos.y + addY);
                Instantiate(controller.spiderwebSwamp, instantiatePos, Quaternion.identity);
                Debug.Log("거미줄 늪 생성");
                Destroy(gameObject);
            }
        }
       
    }
}
