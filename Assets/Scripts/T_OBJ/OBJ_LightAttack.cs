using UnityEditor.U2D.Sprites;
using UnityEngine;

public class OBJ_LightAttack : MonoBehaviour
{
    public float speed = 5f;
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
        t_player = player;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            //적 공격 코드 작성해야함
            if (t_player != null)
            {
                defence = GetComponent<T_Defence>();
            }
            defence.HealthSomeOfDriveGauge(10); //약공시 얻는 드라이브 게이지

            Debug.Log("적 맞음");
            Destroy(gameObject);
        }
    }
}