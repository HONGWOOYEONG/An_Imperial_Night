using UnityEngine;

public class Puppeteer_Hit_A : MonoBehaviour
{
    E_PuppeteerController e_Puppeteer;
    public float[] damage = { 300, 300, 300, 300, 500 }; //피해량
    public float[] groggyDamage = { 270, 270, 270, 270, 320 }; //체간 피해량
    public float[] decreaseDrive = { 250, 250, 250, 250, 500 };//방어 시 드라이브게이지 감소량
    public float[] addGroggy = { 70, 70, 70, 70, 400 }; //방어 시 체간 게이지 증가량

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
        if (e_Puppeteer == null || !e_Puppeteer.isAttaking_A)
        {
            return;
        }
        IDamageReceiver receiver = collision.GetComponent<IDamageReceiver>();
        if (receiver != null)
        {
            int index = e_Puppeteer.currentCount;

            //몬스터 -> 타격 대상
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;

            DamageInfo damageInfo = new DamageInfo
            {
                damage = damage[index],
                damageDir = hitDir,
                knockbackPower = 0,
                stunTime = 0,
                // DamageType damageType, 
                postureDamage = addGroggy[index],
                driveDamage = decreaseDrive[index]
            };
            receiver.ReceiveAttack(damageInfo);
        }
    }
}
