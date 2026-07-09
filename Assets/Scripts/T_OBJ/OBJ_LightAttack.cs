using UnityEditor.U2D.Sprites;
using UnityEngine;

public class OBJ_LightAttack : MonoBehaviour
{
    private float speed = 5f;
    private float damage;
    private Vector2 moveDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, 2f);   
    }

    // Update is called once per frame
    void Update()
    {
        OBJ_Move();
    }

    void OBJ_Move()
    {
        if(moveDirection != Vector2.zero)
        {
            Vector2 myPos = transform.position;
            moveDirection = (myPos - moveDirection).normalized;
            transform.Translate(moveDirection * speed * Time.deltaTime,Space.World);
        }

    }

 

    public void Initialize(float damge, Vector2 targetDir) //초기화
    {
        this.damage = damge;
        this.moveDirection = targetDir; //생성되는 위치에서 적의 위치까지의 방향

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            //적 공격 코드 작성해야함

            Debug.Log("적 맞음");
            Destroy(gameObject);
        }
    }
}
