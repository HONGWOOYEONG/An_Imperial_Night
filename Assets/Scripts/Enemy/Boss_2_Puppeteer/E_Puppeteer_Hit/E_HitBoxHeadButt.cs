using Unity.VisualScripting;
using UnityEngine;

public class E_HitBoxHeadButt : MonoBehaviour
{
    private Rigidbody2D targetRb;
    private float knockBackPower = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("RangedDealer") || collision.gameObject.CompareTag("MeleeDealer"))
        {
            //넉백
            targetRb = collision.gameObject.GetComponent<Rigidbody2D>();
            Vector2 myPos = transform.position;
            Vector2 targetPos = new Vector2(collision.gameObject.transform.position.x, myPos.y);
            Vector2 knockBackDir = (targetPos - myPos).normalized; //적에서 타겟 플레이어 의 방향

            if(targetRb != null)
            {
                targetRb.AddForce(knockBackDir * knockBackPower, ForceMode2D.Impulse);
            }
            
        }
    }
}
