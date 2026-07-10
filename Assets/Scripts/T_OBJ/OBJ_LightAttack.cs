using UnityEditor.U2D.Sprites;
using UnityEngine;

public class OBJ_LightAttack : MonoBehaviour
{
    [SerializeField]float speed = 5f;
    [SerializeField] float healthDG = 10f;

    private float damage;
    private Vector2 moveDirection;
    private GameObject t_player;
    private T_Defence defence;
    void Start()
    {
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        OBJ_Move();
    }

    void OBJ_Move()
    {
        if (moveDirection == Vector2.zero)
            return;

        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

    }

    public void Initialize(float damage, Vector2 targetDir, GameObject player) //초기화
    {
        this.damage = damage;
        moveDirection = targetDir.normalized;
        defence = player.GetComponent<T_Defence>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            //적 공격 코드

            if (defence != null)
            {
                defence.HealthSomeOfDriveGauge(healthDG); //약공시 얻는 드라이브 게이지
            }
            Debug.Log("적 맞음");
            Destroy(gameObject);
        }
    }
   
}