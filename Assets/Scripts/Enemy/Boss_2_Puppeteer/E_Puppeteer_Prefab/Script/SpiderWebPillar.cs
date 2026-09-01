using UnityEngine;


public class SpiderWebPillar : MonoBehaviour
{
    private E_PuppeteerController controller;
    private Rigidbody2D targetRb;
    private float timer;
    private bool isrestraint = false;

    void Start()
    {
        controller = GameObject.FindWithTag("Enemy_Puppeteer").GetComponent<E_PuppeteerController>();
    }

    // Update is called once per frame
    void Update()
    {
     //3초간 멈추는 코드를 작성   
        if (targetRb != null)
        {
            if (isrestraint)
            {
                targetRb.linearVelocity = Vector2.zero; //3초간
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "RangedDealer" || other.gameObject.tag == "DamageDealer")
        {
           if (controller.currentState is P_PatternC_State)
            {
                Debug.Log("C : 플레이어가 거미줄에 걸림");
                controller.ChangeState(controller.states["G"]);
            }
           else if(controller.currentState is P_PatternF_State)
            {
                timer = Time.time + 3f; 
                targetRb = other.GetComponent<Rigidbody2D>();
                
            }
        }
    }
}
