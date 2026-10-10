using UnityEngine;
using UnityEngine.EventSystems;

public class OBJ_FinalAttack : MonoBehaviour
{
    private float speed = 32f;
    private float healthDG = 15f;
    private T_Attack ownerAttack;
    private DamageInfo finalDamageInfo;
    private float timer;
    private float movementTime = 1f;
    private bool isMoving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        OBJ_Move();
        if(timer >= movementTime && !isMoving)
        {
            ownerAttack.movement.ReleaseMovementLock(ownerAttack);
            Debug.Log("이동제어 끝");
            isMoving = true;
        }
    }
    void OBJ_Move()
    {
        if (finalDamageInfo.damageDir == Vector2.zero)
            return;
        //Debug.Log("FinalSpeed :" + speed);
        transform.position += (Vector3)(finalDamageInfo.damageDir * speed * Time.deltaTime);

    }

    public void Initialize(DamageInfo damageInfo, T_Attack t_Attack , Vector2 targetChestPos) //초기화
    {
        this.finalDamageInfo = damageInfo;
        ownerAttack = t_Attack;

        //화살이 바라보는 방향 설정--
        float angle = Mathf.Atan2(finalDamageInfo.damageDir.y, finalDamageInfo.damageDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            //적 공격 코드
            IDamageReceiver receiver = collision.gameObject.GetComponent<IDamageReceiver>();
            if(receiver != null)
            {
                receiver.ReceiveAttack(finalDamageInfo);
                //--적중 시 T_Attack의 OnAttackHit의 드라이브게이지 회복 코드를 가져와서 회복을 하게 만듬--
                ownerAttack?.OnAttackHit(healthDG);
                Debug.Log("finalAttack 적 맞음");
                Destroy(gameObject);
            }
           
        }
    }
}
