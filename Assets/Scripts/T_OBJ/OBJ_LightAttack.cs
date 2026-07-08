using UnityEditor.U2D.Sprites;
using UnityEngine;

public class OBJ_LightAttack : MonoBehaviour
{
    private float speed = 5f;
    private float damage;
    private Vector2 targetPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OBJ_Move();
    }

    void OBJ_Move()
    {
        if(targetPos!=null)
        {
            transform.Translate(targetPos * speed * Time.deltaTime);
            //적이 맞았을 때 적의 정보를 받아서 공격
        }
    }

 

    public void Initialize(float damge, Vector2 targetPos) //초기화
    {
        this.damage = damge;
        this.targetPos = targetPos;

    }
}
