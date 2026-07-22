using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;

//적이 사거리 내에 있으면 타겟팅
//적을 감지해서 플레이어와 가장 가까운 적을 감지
//적이 캐릭터의 시야 이내에 있다면 공격 why? 캐릭터가 위에 있을 수 도 있기 때문에
//사거리 내에 적이 없다면 캐릭터의 바라보는 앞 부분으로 공격

public class RangeCombo
{
    public float lastClickedTime; //마지막으로 클릭을 누른 시간
    public int currentCount = 0; //현재 배열 인덱스
    public float[] damage = { 100, 20, 20, 20, 200 }; //데미지 배열
    public float[] frontDelay = { 15, 4, 4, 4, 15 }; //선딜레이
    public float backDelay = 5f; //후딜레이
}


public class T_Attack : MonoBehaviour
{
    float BASE_FPS = 60f;
    public bool isRight = true;
    Rigidbody2D rb;
    PlayerMovement movement;
    T_Defence defence;
    T_Jump jump;
    [SerializeField] Transform createPos; //공격이 생성되는 position , 플레이어가 부모로 둔 transform을 넣어줘야함

    [Header("약공 or 강공")]
    [SerializeField] float triggerTime = 65f; //약공인지 강공인지 확인하는 시간
    [SerializeField] float triggerTimer = 0f;
    private bool isInputKey = false;
   

    [Header("약공")]
    [SerializeField] float w_attackrange = 20f;//약공 사거리
    [SerializeField] float w_attacktime = 0.5f;//후딜레이가 끝나고 난 후 초 수 이내에 공격해야 다음 데미지로 넘어감
    [SerializeField] float w_viewAngle = 85f;//약공 타겟팅 탐색 각도
    [SerializeField] GameObject w_obj; //약공 오브제 
    private Collider2D w_nearTarget; //가장 가까운 적을 담기위한 변수
    private float w_shortest = float.MaxValue; //가장 짧은 거리의 적을 찾기위해 거리 계산한 값을 넣는 변수
    private bool w_isInsideEnemy = false; //적이 공격 사거리 내에 있나?
    private float w_nextComboRange = 0.5f; //콤보 공격 간격
    private float w_comboExpireTime = 0f;

    [Header("강공")]
    [SerializeField] float s_frontDelay = 5f; //선딜
    [SerializeField] float s_backDelay = 3f; //후딜 
    [SerializeField] GameObject s_obj; //강공 오브제

    [Header("특공")]
    [SerializeField] float sp_drvieDecrease = 100f; //특공 드라이브 감소
    [SerializeField] float sp_stayCount = 1f; //특공 경직 시간
    [SerializeField] float sp_frontDelay = 45f; //선딜
    [SerializeField] float sp_backDelay = 2f; //후딜
    [SerializeField] float sp_atkRange = 5f; //특공 사거리
    private float sp_timer = 0f;
    [SerializeField]private float sp_rayTime = 3f; //특수 레이저 지속 시간
    public bool sp_isAttaking = false; //특공 공격 중일 때 플레이어가 움직이지 못하게 체크
    private bool sp_hasAttacked = false; //레이저 시간 내에 적이 한번만 맞게 하기위함
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
  
    public void OnLightAttack(InputValue value) //약공
    {
        if (value.isPressed) //키 입력을 받았을 때 한번 실행이 됨
        {
            triggerTimer = 0f;
            isInputKey = true;
        }
        else
        {
            Debug.Log("현재 triggerTimer = " + triggerTimer);
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
        if (Time.time > w_comboExpireTime) //콤보 시간 내에 키를 누르지 않으면
        {
            combo.currentCount = 0; //초기화
        }

        FindToNearTarget(); //적 감지
        int attackIndex = combo.currentCount; //현재 인덱스
        if (w_isInsideEnemy && w_nearTarget != null) //적이 공격 사거리, 시야 이내에 있다면
        {
            StartCoroutine(StartLightAttack(attackIndex, w_nearTarget.gameObject.transform.position));
        }
        else //적이 공격 사거리, 시야 내에 없다면
        {
            //플레이어가 보는 방향이 vector2.right인지 vector2.left인지 계산하고 
            //그 위치의 *10한 position을 구해서 인자값으로 넘겨줌

            Vector2 lookDir = Vector2.right * movement.FacingDirection;
            Vector2 forwardPos = (Vector2)createPos.position + (lookDir * 10f); //위치

            StartCoroutine(StartLightAttack(attackIndex, forwardPos));
        }
        Debug.Log("현재 콤보 인덱스 = "+attackIndex);
        combo.currentCount = (combo.currentCount + 1) % 5;
        w_comboExpireTime = Time.time + w_nextComboRange; //지금 공격 시점부터 w_attacktime
    }

    //투사체 생성과 투사체에게 정보 넘겨주기
    private IEnumerator StartLightAttack(int index ,Vector2 targetPos) // targetPos = 위치 값을 넘겨줘야함
    {
        yield return new WaitForSeconds(combo.frontDelay[index] /BASE_FPS); //선 딜레이

        Vector2 newPos = createPos.position; //약공격이 생성되는 위치
        GameObject obj_lightatk = Instantiate(w_obj, newPos, Quaternion.identity); //투사체 생성
        OBJ_LightAttack atkInit = obj_lightatk.GetComponent<OBJ_LightAttack>(); //투사체의 스크립트 가져오기
        if (atkInit != null)
        {
            //적의 위치를 가져와서 날아가야할 방향을 설정해줌
            Vector2 Pos = (targetPos - newPos).normalized; //방향
            atkInit.Initialize(combo.damage[index], Pos, this.gameObject); //투사체 공격, 가야할 방향(위치) 저장
        }

        yield return new WaitForSeconds(combo.backDelay/BASE_FPS); //후 딜레이
    }


    //overlap의 사거리 내에 있는 적 중에 가장 가까운 적을 찾음
    private void FindToNearTarget()
    {
        w_shortest = float.MaxValue; //탐색 할때마다 초기화

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, w_attackrange);
        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                Vector2 targetPos = target.transform.position; //위치
                Vector2 playerPos = transform.position; //위치


                Vector2 dir = (targetPos - playerPos).normalized; //방향
                Vector2 myForward = transform.right; //방향
                float angle = Vector2.Angle(myForward, dir); //각도
                //시야 이내에 있음
                if (angle <= w_viewAngle )
                {
                    Debug.Log("적 추가" + playerPos);
                    //플레이어와 적의 거리를 비교
                    float distance = Vector2.Distance(playerPos, targetPos);
                    //가장 짧은걸 비교
                    if (distance < w_shortest)
                    {
                        w_shortest = distance;
                        w_nearTarget = target;
                    }
                }
            }
        }
        if (w_nearTarget != null )//타겟이 null이 아니고 적의 거리가 공격 사거리보다 작다면
        {
            w_isInsideEnemy = true;
        }

        if (w_nearTarget != null)
        {
            Debug.Log($"최종 타겟 발견: {w_nearTarget.name}, 거리: {w_shortest}, 사거리내 유무: {w_isInsideEnemy}");
        }
        else //적을 찾지 못하면 변수들 초기화
        {
            Debug.Log($"최종 타겟 발견: 없음, 거리: {w_shortest}, 사거리내 유무: {w_isInsideEnemy}");
            w_nearTarget = null;
            w_isInsideEnemy = false;
        }
    }



    public void HeavyAttack()//강공
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

    public void OnAbility(InputValue value) //특공
    {
        bool isbunout = defence.GetIsbunout();
        float currentDriveGauge = defence.GetCurrentDriveGauge();
        if (value.isPressed && !isbunout && currentDriveGauge > sp_drvieDecrease && !sp_isAttaking)
        {
            Debug.Log("특공 시작");
            defence.DecreaseDriveGauge(sp_drvieDecrease); //드라이브 게이지 감소
            StartCoroutine(SpecialAttack());
        }
    }

    private IEnumerator SpecialAttack()
    {
        sp_isAttaking = true;
        float normalrgavity = rb.gravityScale;
        if (movement != null) //스크립트를 잠깐 꺼줌
        {
            movement.enabled = false;
        }
        rb.linearVelocity = Vector2.zero; //이동 못하게 함
        if (jump.isJumping) //점프 중이라면
        {
            rb.gravityScale = 0f;
        }

        Vector2 crtPos = (createPos.position);
        yield return new WaitForSeconds(sp_frontDelay / BASE_FPS); //선 딜레이 동안에는 애니메이션 아무것도 안넣음

        sp_timer = 0f;
        float keepRayTime = sp_rayTime / BASE_FPS; //광선 유지 시간
         while(sp_timer < keepRayTime) 
        {
            //레이저가 발사되는 시간 안에 적을 한번만 공격하기 위해서
               sp_timer += Time.deltaTime;
                RaycastHit2D hit = Physics2D.Raycast(crtPos, Vector2.right, sp_atkRange); //crtPos에서 Vector2.right방향, sp_atkRange사거리
                Debug.DrawRay(crtPos, Vector2.right * sp_atkRange,Color.yellow,keepRayTime ); //레이
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Enemy") && !sp_hasAttacked) //맞은 오브젝트가 적이고 공격을 안했다면
                    {
                        Debug.Log(hit.collider.name);
                        //공격
                       sp_hasAttacked = true;
                    }
                }
                yield return null;
            }
        sp_hasAttacked = false;
        yield return new WaitForSeconds(sp_backDelay/BASE_FPS); //후 딜
        if (movement != null) 
        {
            movement.enabled = true;
        }
        rb.gravityScale = normalrgavity;
        sp_isAttaking = false;
        
    }
    
    private void knockback(float amount) //넉백
    {
        //오른쪽을 보고 있으면
        if (movement.FacingDirection==1)
        {

            rb.AddForce(Vector2.left * amount, ForceMode2D.Impulse);
        }
        else  //왼쪽을 보고 있으면 
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
