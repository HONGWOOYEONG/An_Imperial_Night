using System.Drawing;
using UnityEngine;

public class OBJ_HeavyAttack : MonoBehaviour
{
    T_DriveGauge t_DriveGauge;
    BoxCollider2D boxCollider = null;
 
    [SerializeField] float healthDG = 30f;//회복되는 드라이브 게이지
    [SerializeField] float destroyTime = 4f;
    float damageMultiplier = 0.15f; //체력피해와 체간피해를 +15% 
    private float healthDamage = 7f; //기본 hp 데미지
    private float groggyDamage = 10; //기본 groggy 데미지
    private float time = 2.8f; //적중 시간
    private float timer = 0; //적중 시간 체크 

    [Header("늘어나는 오브젝트")]
    private Vector2 direction = Vector2.zero;
    private float maxWidth = 8f;
    private float growSpeed = 4f;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject tPlayer = GameObject.FindGameObjectWithTag("RangedDealer");
        t_DriveGauge = tPlayer.GetComponent<T_DriveGauge>();
        boxCollider = GetComponent<BoxCollider2D>();
        Destroy(this.gameObject, destroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        ExpandOnKnockback();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (timer >= time) return;
        if (collision.collider.CompareTag("Enemy"))
        {
            TakeDamage(collision.collider);
            if (t_DriveGauge != null)
            {
                t_DriveGauge.HealthSomeOfDriveGauge(healthDG);
            }
            Destroy(gameObject);
        }
    }


    //정보 가져올 때 함수
    public void Initialize( Vector2 knockbackDir)
    {
       
        direction = knockbackDir;
    }

    //넉백 시 늘어나는 오브젝트 함수
   private void ExpandOnKnockback() 
    {
        if (transform.localScale.x >= maxWidth) return;
        
        float growAmount = growSpeed * Time.deltaTime;

        //collider, localscale 늘리기
        Vector3 scale = transform.localScale;
        scale.x += growAmount;
        transform.localScale = scale;
        // 한쪽 끝이 고정되도록 위치 보정
        transform.position += new Vector3(
            direction.x * growAmount * 0.5f,
            0f,
            0f
        );

        //offset보정 
        Vector2 offset = boxCollider.offset;
        offset.x += growAmount * 0.5f * direction.x;       
        boxCollider.offset = offset;

        //오브젝트랑 부딪혔을 때는 어떻게 처리할건지? 오브젝트 자체는 늘어나는데 플레이어가 멈추니까 조금 이상하게 보임
    }
    private void ActivateMagicTrail() //마력을 남기는 함수
    {

    }

    //대상이 받는 체력피해와 체간피해를 +15% 더 받는 함수
    private void TakeDamage(Collider2D collider)
    {
        float minusHp = (healthDamage) * (1 + damageMultiplier);
        float minusGroggy = (groggyDamage) * (1 + damageMultiplier);

        //collider.hp -= minusHp;
        //collider.groggy += minusGroggy;
    }

}
