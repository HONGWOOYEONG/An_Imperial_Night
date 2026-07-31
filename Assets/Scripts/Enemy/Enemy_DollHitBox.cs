using UnityEngine;

public class Enemy_DollHitBox : MonoBehaviour
{
    E_PuppeteerController e_pupperteert;
    private float currentDamage = 0f;
    public void SetDamage(float damage) //이 히트박스가 켜져있는 동안 데미지 값은 currentDamage;
    {
        currentDamage = damage;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DamageDealer"))
        {  
            //근딜 공격 코드 작성
            //damageDealer.hp -= damage;
        }
        if (collision.gameObject.CompareTag("RangedDealer"))
        {
           
            //원딜 공격 코드 작성
            //RangedDealer.hp -= damage;
        }
    }
}
