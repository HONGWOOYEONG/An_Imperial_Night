using UnityEngine;
using UnityEngine.EventSystems;

public class OBJ_FinalAttack : MonoBehaviour
{
    [SerializeField] float speed = 9f;
    [SerializeField] float healthDG = 15f;
    private T_Attack ownerAttack;
    private DamageInfo damageInfo;
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
            ownerAttack.movement.SetActiveMovingTrue();
            isMoving = true;
        }
    }
    void OBJ_Move()
    {
        if (damageInfo.damageDir == Vector2.zero)
            return;

        transform.position += (Vector3)(damageInfo.damageDir * speed * Time.deltaTime);

    }

    public void Initialize(DamageInfo damageInfo, T_Attack t_Attack) //초기화
    {
        this.damageInfo = damageInfo;
        ownerAttack = t_Attack;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            //적 공격 코드
            //--마력의 흔적--

            //--적중 시 T_Attack의 OnAttackHit의 드라이브게이지 회복 코드를 가져와서 회복을 하게 만듬--
            ownerAttack?.OnAttackHit(healthDG);
            Debug.Log("finalAttack 적 맞음");
            Destroy(gameObject);
        }
    }
}
