using UnityEngine;


public class SpiderWebPillar : MonoBehaviour
{
    private E_PuppeteerController controller;

    [Header("knockBack")]
    private PlayerMovement playerMovement;
    private bool isKnockBackActive = false; //넉백 인정 감지 변수
    private float knockBackTime = 0.5f; //넉백 인정 시간
    private float knockBackTimer = 0;

    void Start()
    {
        isKnockBackActive = true;
        controller = GameObject.FindWithTag("Enemy_Puppeteer").GetComponent<E_PuppeteerController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        knockBackTimer += Time.deltaTime;
        if (knockBackTimer >= knockBackTime)
        {
            isKnockBackActive=false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.tag == "RangedDealer" || other.gameObject.tag == "MeleeDealer")
        {
           if (controller.currentState is P_PatternC_State)
            {
                Debug.Log("C : 플레이어가 거미줄에 걸림");
                controller.ChangeState(controller.states["G"]);
            }
           if(controller.currentState is P_PatternF_State)
            {
                if (isKnockBackActive)
                {
                    playerMovement = other.GetComponent<PlayerMovement>();
                    if(playerMovement != null)
                    {
                        Vector2 myPos = controller.transform.position;
                        Vector2 targetPos = other.transform.position;
                        float dirX = (targetPos.x - myPos.x) > 0 ? 1 : -1;
                        Vector2 dirToTarget = new Vector2(dirX, 0f);
                        playerMovement.KnockBack(dirToTarget);
                    }
                    //거미줄 벽 생성시점에, 캐릭터가 거미줄 벽 위에 있으면 양 옆으로 밀려납니다.
                }
            }
        }
        else
        {
            if(controller.currentState is P_PatternC_State)
            {
                controller.ChangeState(controller.states["idle"]);
            } 
        }
    }
}
