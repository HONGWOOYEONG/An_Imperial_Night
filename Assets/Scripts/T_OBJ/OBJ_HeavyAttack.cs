using UnityEngine;

public class OBJ_HeavyAttack : MonoBehaviour
{
    [SerializeField] float speed = 10f; //강공 스피드
    [SerializeField] float healthDG = 30f;//회복되는 드라이브 게이지
    [SerializeField] float destroyTime = 2f;
    T_Defence defence;
    GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(this.gameObject, destroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    public void Initialize(GameObject player)
    {
        this.player = player;
        defence = player.GetComponent<T_Defence>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            if (defence != null)
            {
                defence.HealthSomeOfDriveGauge(healthDG);
            }
            Destroy(gameObject);
        }
    }
 
}
