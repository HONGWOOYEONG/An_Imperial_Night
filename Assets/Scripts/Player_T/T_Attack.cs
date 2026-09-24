using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RangeCombo
{
    public float lastClickedTime; 
    public int currentCount = 0; 
    public float[] damage = { 100, 20, 20, 20, 200 }; 
    public float[] frontDelay = { 15, 4, 4, 4, 15 }; 
    public float backDelay = 5f;
}


public class T_Attack : MonoBehaviour
{
    float BASE_FPS = 60f;
    public bool isRight = true;
    Rigidbody2D rb = null;
    PlayerMovement movement = null;
    T_DriveGauge t_DriveGauge = null;
    T_Jump jump = null;
    [SerializeField] Transform createPos = null;
    [Header("��� or ����")]
    [SerializeField] float triggerTime = 65f; 
    [SerializeField] float triggerTimer = 0f;
    private bool isInputKey = false;
   

    [Header("약공")]
    [SerializeField] float w_attackrange = 20f;
    [SerializeField] float w_attacktime = 0.5f;
    [SerializeField] float w_viewAngle = 85f;
    [SerializeField] GameObject w_obj; 
    private Collider2D w_nearTarget; 
    private float w_shortest = float.MaxValue; 
    private bool w_isInsideEnemy = false;
    private float w_nextComboRange = 0.5f;
    private float w_comboExpireTime = 0f;

    [Header("강공")]
    [SerializeField] float s_frontDelay = 90f; 
    [SerializeField] float s_backDelay = 0.8f; 
    [SerializeField] GameObject s_obj;

    [Header("특공")]
    [SerializeField] float sp_drvieDecrease = 100f; 
    [SerializeField] float sp_stayCount = 1f;
    [SerializeField] float sp_frontDelay = 45f;
    [SerializeField] float sp_backDelay = 2f; 
    [SerializeField] float sp_atkRange = 5f;
    private float sp_timer = 0f;
    [SerializeField]private float sp_rayTime = 3f; 
    public bool sp_isAttaking = false;
    private bool sp_hasAttacked = false;
    RangeCombo combo;
    void Start()
    {
        combo = new RangeCombo();
        movement = GetComponent<PlayerMovement>();
        t_DriveGauge = GetComponent<T_DriveGauge>();
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
  
    public void OnLightAttack(InputValue value) 
    {
        if (value.isPressed)
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
                LightAttack(); //짧게 누르면 약공
            }
            else
            {
                HeavyAttack(); //길게누르면 강공
            }
           
        }
    }
    private void LightAttack()
    {
        w_nearTarget = null;
        w_isInsideEnemy = false;
        if (Time.time > w_comboExpireTime) 
        {
            combo.currentCount = 0;
        }

        FindToNearTarget(); 
        int attackIndex = combo.currentCount;
        if (w_isInsideEnemy && w_nearTarget != null) 
        {
            StartCoroutine(StartLightAttack(attackIndex, w_nearTarget.gameObject.transform.position));
        }
        else 
        {
            Vector2 lookDir = Vector2.right * movement.FacingDirection;
            Vector2 forwardPos = (Vector2)createPos.position + (lookDir * 10f); 

            StartCoroutine(StartLightAttack(attackIndex, forwardPos));
        }
        combo.currentCount = (combo.currentCount + 1) % 5;
        w_comboExpireTime = Time.time + w_nextComboRange; 
    }

 
    private IEnumerator StartLightAttack(int index ,Vector2 targetPos) 
    {
        yield return new WaitForSeconds(combo.frontDelay[index] /BASE_FPS); 

        Vector2 newPos = createPos.position; 
        GameObject obj_lightatk = InstantiateObject(w_obj, newPos); 
        OBJ_LightAttack atkInit = obj_lightatk.GetComponent<OBJ_LightAttack>(); 
        if (atkInit != null)
        {
            Vector2 Pos = (targetPos - newPos).normalized; 
            atkInit.Initialize(combo.damage[index], Pos, this.gameObject); 
        }

        yield return new WaitForSeconds(combo.backDelay/BASE_FPS); 
    }


    private void FindToNearTarget()
    {
        w_shortest = float.MaxValue; 

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, w_attackrange);
        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                Vector2 targetPos = target.transform.position; 
                Vector2 playerPos = transform.position; 

                Vector2 dir = (targetPos - playerPos).normalized; 
                Vector2 myForward = transform.right; 
                float angle = Vector2.Angle(myForward, dir); //바라보는 시야각도
                if (angle <= w_viewAngle )
                {                   
                    float distance = Vector2.Distance(playerPos, targetPos);
                    if (distance < w_shortest)
                    {
                        w_shortest = distance;
                        w_nearTarget = target;
                    }
                }
            }
        }

        if (w_nearTarget != null)
        {
        }
        else 
        {
            w_nearTarget = null;
        }
    }



    public void HeavyAttack()
    {
        StartCoroutine(StrongAttack());
    }

    private IEnumerator StrongAttack()
    {
        yield return new WaitForSeconds(s_frontDelay/BASE_FPS);

        Vector2 newPos = createPos.position; //오브젝트 생성 위치
        Vector2 attackerPos = transform.position; // 공격자 위치
        Vector2 knockbackDir = -((newPos - attackerPos).normalized); //넉백 방향

        GameObject obj_heavyAttack = InstantiateObject(s_obj, newPos);
        OBJ_HeavyAttack heavyAttack = obj_heavyAttack.GetComponent<OBJ_HeavyAttack>();
        heavyAttack.Initialize(knockbackDir);
        movement.KnockBack(knockbackDir);

        yield return new WaitForSeconds(s_backDelay/BASE_FPS);
    }

    public void OnAbility(InputValue value)
    {
        bool isbunout = t_DriveGauge.isBunOut;
        float currentDriveGauge = t_DriveGauge.driveGauge;
        if (value.isPressed && !isbunout && currentDriveGauge > sp_drvieDecrease && !sp_isAttaking)
        {
            Debug.Log("Ư�� ����");
            t_DriveGauge.DecreaseDriveGauge(sp_drvieDecrease); 
            StartCoroutine(SpecialAttack());
        }
    }

    private IEnumerator SpecialAttack()
    {
        sp_isAttaking = true;
        float normalrgavity = rb.gravityScale;
        if (movement != null) 
        {
            movement.enabled = false;  // 이동 및 점프 제어 비활성화
        }
        rb.linearVelocity = Vector2.zero; 
        if (jump.isJumping) 
        {
            rb.gravityScale = 0f;
        }

        Vector2 crtPos = (createPos.position);
        yield return new WaitForSeconds(sp_frontDelay / BASE_FPS); 

        sp_timer = 0f;
        float keepRayTime = sp_rayTime / BASE_FPS; 
         while(sp_timer < keepRayTime) 
        {
               sp_timer += Time.deltaTime;
                RaycastHit2D hit = Physics2D.Raycast(crtPos, Vector2.right, sp_atkRange);
                Debug.DrawRay(crtPos, Vector2.right * sp_atkRange,Color.yellow,keepRayTime );
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Enemy") && !sp_hasAttacked) 
                    {
                        Debug.Log(hit.collider.name);
                       
                       sp_hasAttacked = true;
                    }
                }
                yield return null;
            }
        sp_hasAttacked = false;
        yield return new WaitForSeconds(sp_backDelay/BASE_FPS); 
        if (movement != null) 
        {
            movement.enabled = true; //원래 상태 복구
        }
        rb.gravityScale = normalrgavity;
        sp_isAttaking = false;
        
    }
    
    private GameObject InstantiateObject(GameObject obj, Vector2 createPos)
    {
        return Instantiate(obj, createPos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, w_attackrange);
    }

}
