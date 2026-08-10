using UnityEngine;

public class Puppeteer_Hit_B : MonoBehaviour
{
    E_PuppeteerController e_Puppeteer;
    float[] damage = { 400, 300 };
    float addGroggy = 100;
    float[] decreaseDrive = { 300, 250 };
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
        if (e_Puppeteer == null || !e_Puppeteer.isAttaking_B)
        {
            return;
        }
        int index = e_Puppeteer.count;

        IDamageReceiver receiver = collision.GetComponent<IDamageReceiver>();
        if (receiver != null)
        {

            //몬스터 -> 타격 대상
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;

            DamageInfo damageInfo = new DamageInfo
            {
                damage = damage[index],
                damageDir = hitDir,
                knockbackPower = 0,
                stunTime = 0,
                // DamageType damageType, 
                postureDamage = addGroggy,
                driveDamage = decreaseDrive[index]
            };
            receiver.ReceiveAttack(damageInfo);
        }
    }
}
