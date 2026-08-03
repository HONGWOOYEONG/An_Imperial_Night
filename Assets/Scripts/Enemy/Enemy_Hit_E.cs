using UnityEngine;

public class Enemy_Hit_E : MonoBehaviour
{
    E_PuppeteerController e_Puppeteer;
    float damage = 400f;
    float addGroggy = 100f;
    float decreaseDrive = 200f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        e_Puppeteer = GetComponent<E_PuppeteerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (e_Puppeteer == null || !e_Puppeteer.isAttaking_E)
        {
            return;
        }
        IDamageReceiver receiver = collision.GetComponent<IDamageReceiver>();
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (receiver != null)
        {

            //몬스터 -> 타격 대상
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;

            DamageInfo damageInfo = new DamageInfo
            {
                damage = damage,
                damageDir = hitDir,
                knockbackPower = 0,
                stunTime = 0,
                // DamageType damageType, 
                postureDamage = addGroggy,
                driveDamage = decreaseDrive
            };
            receiver.ReceiveAttack(damageInfo);
        
        }
    }
}
