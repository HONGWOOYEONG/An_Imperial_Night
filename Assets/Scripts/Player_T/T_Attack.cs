using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;

//���� ��Ÿ� ���� ������ Ÿ����
//���� �����ؼ� �÷��̾�� ���� ����� ���� ����
//���� ĳ������ �þ� �̳��� �ִٸ� ���� why? ĳ���Ͱ� ���� ���� �� �� �ֱ� ������
//��Ÿ� ���� ���� ���ٸ� ĳ������ �ٶ󺸴� �� �κ����� ����

public class RangeCombo
{
    public float lastClickedTime; //���������� Ŭ���� ���� �ð�
    public int currentCount = 0; //���� �迭 �ε���
    public float[] damage = { 100, 20, 20, 20, 200 }; //������ �迭
    public float[] frontDelay = { 15, 4, 4, 4, 15 }; //��������
    public float backDelay = 5f; //�ĵ�����
}


public class T_Attack : MonoBehaviour
{
    float BASE_FPS = 60f;
    public bool isRight = true;
    Rigidbody2D rb;
    PlayerMovement movement;
    T_Defence defence;
    T_Jump jump;
    [SerializeField] Transform createPos; //������ �����Ǵ� position , �÷��̾ �θ�� �� transform�� �־������

    [Header("��� or ����")]
    [SerializeField] float triggerTime = 65f; //������� �������� Ȯ���ϴ� �ð�
    [SerializeField] float triggerTimer = 0f;
    private bool isInputKey = false;
   

    [Header("���")]
    [SerializeField] float w_attackrange = 20f;//��� ��Ÿ�
    [SerializeField] float w_attacktime = 0.5f;//�ĵ����̰� ������ �� �� �� �� �̳��� �����ؾ� ���� �������� �Ѿ
    [SerializeField] float w_viewAngle = 85f;//��� Ÿ���� Ž�� ����
    [SerializeField] GameObject w_obj; //��� ������ 
    private Collider2D w_nearTarget; //���� ����� ���� ������� ����
    private float w_shortest = float.MaxValue; //���� ª�� �Ÿ��� ���� ã������ �Ÿ� ����� ���� �ִ� ����
    private bool w_isInsideEnemy = false; //���� ���� ��Ÿ� ���� �ֳ�?
    private float w_nextComboRange = 0.5f; //�޺� ���� ����
    private float w_comboExpireTime = 0f;

    [Header("����")]
    [SerializeField] float s_frontDelay = 5f; //����
    [SerializeField] float s_backDelay = 3f; //�ĵ� 
    [SerializeField] GameObject s_obj; //���� ������

    [Header("Ư��")]
    [SerializeField] float sp_drvieDecrease = 100f; //Ư�� ����̺� ����
    [SerializeField] float sp_stayCount = 1f; //Ư�� ���� �ð�
    [SerializeField] float sp_frontDelay = 45f; //����
    [SerializeField] float sp_backDelay = 2f; //�ĵ�
    [SerializeField] float sp_atkRange = 5f; //Ư�� ��Ÿ�
    private float sp_timer = 0f;
    [SerializeField]private float sp_rayTime = 3f; //Ư�� ������ ���� �ð�
    public bool sp_isAttaking = false; //Ư�� ���� ���� �� �÷��̾ �������� ���ϰ� üũ
    private bool sp_hasAttacked = false; //������ �ð� ���� ���� �ѹ��� �°� �ϱ�����
    RangeCombo combo;
    void Start()
    {
        combo = new RangeCombo();
        movement = GetComponent<PlayerMovement>();
        defence = GetComponent<T_Defence>();
        jump = GetComponent<T_Jump>();
        rb = GetComponent<Rigidbody2D>();

        triggerTime /= BASE_FPS;
    }


    void Update()
    {
        if (isInputKey)
        {
            triggerTimer += Time.deltaTime; ;
        }
    }
  
    public void OnLightAttack(InputValue value) //���
    {
        if (value.isPressed) //Ű �Է��� �޾��� �� �ѹ� ������ ��
        {
            triggerTimer = 0f;
            isInputKey = true;
        }
        else
        {
            Debug.Log("���� triggerTimer = " + triggerTimer);
            isInputKey = false;
            if(triggerTimer <= triggerTime)
            {
                LightAttack();
            }
            else
            {
                HeavyAttack();
            }
           
        }
    }
    private void LightAttack()
    {
        w_nearTarget = null;
        w_isInsideEnemy = false;
        if (Time.time > w_comboExpireTime) //�޺� �ð� ���� Ű�� ������ ������
        {
            combo.currentCount = 0; //�ʱ�ȭ
        }

        FindToNearTarget(); //�� ����
        int attackIndex = combo.currentCount; //���� �ε���
        if (w_isInsideEnemy && w_nearTarget != null) //���� ���� ��Ÿ�, �þ� �̳��� �ִٸ�
        {
            StartCoroutine(StartLightAttack(attackIndex, w_nearTarget.gameObject.transform.position));
        }
        else //���� ���� ��Ÿ�, �þ� ���� ���ٸ�
        {
            //�÷��̾ ���� ������ vector2.right���� vector2.left���� ����ϰ� 
            //�� ��ġ�� *10�� position�� ���ؼ� ���ڰ����� �Ѱ���

            Vector2 lookDir = Vector2.right * movement.FacingDirection;
            Vector2 forwardPos = (Vector2)createPos.position + (lookDir * 10f); //��ġ

            StartCoroutine(StartLightAttack(attackIndex, forwardPos));
        }
        Debug.Log("���� �޺� �ε��� = "+attackIndex);
        combo.currentCount = (combo.currentCount + 1) % 5;
        w_comboExpireTime = Time.time + w_nextComboRange; //���� ���� �������� w_attacktime
    }

    //����ü ������ ����ü���� ���� �Ѱ��ֱ�
    private IEnumerator StartLightAttack(int index ,Vector2 targetPos) // targetPos = ��ġ ���� �Ѱ������
    {
        yield return new WaitForSeconds(combo.frontDelay[index] /BASE_FPS); //�� ������

        Vector2 newPos = createPos.position; //������� �����Ǵ� ��ġ
        GameObject obj_lightatk = Instantiate(w_obj, newPos, Quaternion.identity); //����ü ����
        OBJ_LightAttack atkInit = obj_lightatk.GetComponent<OBJ_LightAttack>(); //����ü�� ��ũ��Ʈ ��������
        if (atkInit != null)
        {
            //���� ��ġ�� �����ͼ� ���ư����� ������ ��������
            Vector2 Pos = (targetPos - newPos).normalized; //����
            atkInit.Initialize(combo.damage[index], Pos, this.gameObject); //����ü ����, ������ ����(��ġ) ����
        }

        yield return new WaitForSeconds(combo.backDelay/BASE_FPS); //�� ������
    }


    //overlap�� ��Ÿ� ���� �ִ� �� �߿� ���� ����� ���� ã��
    private void FindToNearTarget()
    {
        w_shortest = float.MaxValue; //Ž�� �Ҷ����� �ʱ�ȭ

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, w_attackrange);
        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                Vector2 targetPos = target.transform.position; //��ġ
                Vector2 playerPos = transform.position; //��ġ


                Vector2 dir = (targetPos - playerPos).normalized; //����
                Vector2 myForward = transform.right; //����
                float angle = Vector2.Angle(myForward, dir); //����
                //�þ� �̳��� ����
                if (angle <= w_viewAngle )
                {
                    Debug.Log("�� �߰�" + playerPos);
                    //�÷��̾�� ���� �Ÿ��� ��
                    float distance = Vector2.Distance(playerPos, targetPos);
                    //���� ª���� ��
                    if (distance < w_shortest)
                    {
                        w_shortest = distance;
                        w_nearTarget = target;
                    }
                }
            }
        }
        if (w_nearTarget != null )//Ÿ���� null�� �ƴϰ� ���� �Ÿ��� ���� ��Ÿ����� �۴ٸ�
        {
            w_isInsideEnemy = true;
        }

        if (w_nearTarget != null)
        {
            Debug.Log($"���� Ÿ�� �߰�: {w_nearTarget.name}, �Ÿ�: {w_shortest}, ��Ÿ��� ����: {w_isInsideEnemy}");
        }
        else //���� ã�� ���ϸ� ������ �ʱ�ȭ
        {
            Debug.Log($"���� Ÿ�� �߰�: ����, �Ÿ�: {w_shortest}, ��Ÿ��� ����: {w_isInsideEnemy}");
            w_nearTarget = null;
            w_isInsideEnemy = false;
        }
    }



    public void HeavyAttack()//����
    {
              // knockback(3f);
            StartCoroutine(StrongAttack());        
    }

    private IEnumerator StrongAttack()
    {
        yield return new WaitForSeconds(s_frontDelay/BASE_FPS);
        Vector2 newPos = createPos.position;
        GameObject obj_heavyAttack = Instantiate(s_obj, newPos, Quaternion.identity);
        OBJ_HeavyAttack heavyAttack = obj_heavyAttack.GetComponent<OBJ_HeavyAttack>();
        heavyAttack.Initialize(this.gameObject);
        yield return new WaitForSeconds(s_backDelay/BASE_FPS);
    }

    public void OnAbility(InputValue value) //Ư��
    {
        bool isbunout = defence.GetIsbunout();
        float currentDriveGauge = defence.GetCurrentDriveGauge();
        if (value.isPressed && !isbunout && currentDriveGauge > sp_drvieDecrease && !sp_isAttaking)
        {
            Debug.Log("Ư�� ����");
            defence.DecreaseDriveGauge(sp_drvieDecrease); //����̺� ������ ����
            StartCoroutine(SpecialAttack());
        }
    }

    private IEnumerator SpecialAttack()
    {
        sp_isAttaking = true;
        float normalrgavity = rb.gravityScale;
        if (movement != null) //��ũ��Ʈ�� ��� ����
        {
            movement.enabled = false;
        }
        rb.linearVelocity = Vector2.zero; //�̵� ���ϰ� ��
        if (jump.isJumping) //���� ���̶��
        {
            rb.gravityScale = 0f;
        }

        Vector2 crtPos = (createPos.position);
        yield return new WaitForSeconds(sp_frontDelay / BASE_FPS); //�� ������ ���ȿ��� �ִϸ��̼� �ƹ��͵� �ȳ���

        sp_timer = 0f;
        float keepRayTime = sp_rayTime / BASE_FPS; //���� ���� �ð�
         while(sp_timer < keepRayTime) 
        {
            //�������� �߻�Ǵ� �ð� �ȿ� ���� �ѹ��� �����ϱ� ���ؼ�
               sp_timer += Time.deltaTime;
                RaycastHit2D hit = Physics2D.Raycast(crtPos, Vector2.right, sp_atkRange); //crtPos���� Vector2.right����, sp_atkRange��Ÿ�
                Debug.DrawRay(crtPos, Vector2.right * sp_atkRange,Color.yellow,keepRayTime ); //����
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Enemy") && !sp_hasAttacked) //���� ������Ʈ�� ���̰� ������ ���ߴٸ�
                    {
                        Debug.Log(hit.collider.name);
                        //����
                       sp_hasAttacked = true;
                    }
                }
                yield return null;
            }
        sp_hasAttacked = false;
        yield return new WaitForSeconds(sp_backDelay/BASE_FPS); //�� ��
        if (movement != null) 
        {
            movement.enabled = true;
        }
        rb.gravityScale = normalrgavity;
        sp_isAttaking = false;
        
    }
    
    private void knockback(float amount) //�˹�
    {
        //�������� ���� ������
        if (movement.FacingDirection==1)
        {

            rb.AddForce(Vector2.left * amount, ForceMode2D.Impulse);
        }
        else  //������ ���� ������ 
        {
            rb.AddForce(Vector2.right * amount, ForceMode2D.Impulse);
        }
       
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, w_attackrange);
    }

}
