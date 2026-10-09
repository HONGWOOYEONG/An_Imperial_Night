using UnityEditor.U2D.Sprites;
using UnityEngine;

public class OBJ_LightAttack : MonoBehaviour
{
    private float speed = 35f;
    private float healthDG = 10f;
    private T_Attack ownerAttack;
    private DamageInfo lightDamageInfo;

    void Start()
    {
        Destroy(gameObject, 0.52f);
    }

    void Update()
    {
        OBJ_Move();
    }

    void OBJ_Move()
    {
        if (lightDamageInfo.damageDir == Vector2.zero)
            return;

        Debug.Log("LightSpeed :" + speed);
        transform.position += (Vector3)(lightDamageInfo.damageDir * speed * Time.deltaTime);
       
    }

    //--Ȱ�� ������Ʈ�� ������ �� DamageInfo�� �����ͼ� DamageInfo�� �������� ������ �ϰ� ��--
    public void Initialize(DamageInfo damageInfo, T_Attack t_Attack , Vector2 targetChestPos) //�ʱ�ȭ
    {
        this.lightDamageInfo = damageInfo;
        ownerAttack = t_Attack;

        //ȭ���� �ٶ󺸴� ���� ����--
        float angle = Mathf.Atan2(lightDamageInfo.damageDir.y, lightDamageInfo.damageDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            IDamageReceiver receiver = collision.GetComponent<IDamageReceiver>();

            if (receiver != null)
            {
                //--������ ���� ������ �ѱ�--
                receiver.ReceiveAttack(lightDamageInfo);

                //--���� �� T_Attack�� OnAttackHit�� ����̺������ ȸ�� �ڵ带 �����ͼ� ȸ���� �ϰ� ����--
                ownerAttack?.OnAttackHit(healthDG);
                Debug.Log("normalAttack 맞음");
                Destroy(gameObject);
            }

        }
}

   
      
}