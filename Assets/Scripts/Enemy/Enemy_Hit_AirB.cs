using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Enemy_Hit_AirB : MonoBehaviour
{
    E_PuppeteerController e_Puppeteer;
    float damage = 300f;
    float addGroggy = 100f;
    float decreaseDrive = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        e_Puppeteer = GetComponent<E_PuppeteerController>();
    }
    void Update()
    {
        
    }
private void OnTriggerEnter2D(Collider2D collision)
{
    if (e_Puppeteer == null || !e_Puppeteer.isAttaking_AirB)
    {
        return;
    }

    IDamageReceiver receiver = collision.GetComponent<IDamageReceiver>();
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
