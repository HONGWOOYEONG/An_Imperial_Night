using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

//적이 사거리 내에 있으면 타겟팅
//적을 감지해서 플레이어와 가장 가까운 적을 감지
//적이 캐릭터의 시야 이내에 있다면 공격 why? 캐릭터가 위에 있을 수 도 있기 때문에
//사거리 내에 적이 없다면 캐릭터의 바라보는 앞 부분으로 공격

public class Combo
{
    public float lastClickedTime; //마지막으로 클릭을 누른 시간
    public int currentCount = 0; //현재 배열 인덱스
    public float[] damage = { 100, 20, 20, 20, 200 }; //데미지 배열
    public float[] frontDeley = { 15, 4, 4, 4, 15 }; //선딜레이
    public float backDeley = 5f; //후딜레이
}


public class T_Attack : MonoBehaviour
{
    float BASE_FPS = 30f;

    [Header("약공")]
    [SerializeField] float w_attackrange = 3f;//약공 사거리
    [SerializeField] float w_attacktime = 0.5f;//후딜레이가 끝나고 난 후 초 수 이내에 공격해야 다음 데미지로 넘어감
    float viewAngle = 90f;//약공 타겟팅 탐색 각도
    [SerializeField] GameObject obj; //약공 오브제 
    [SerializeField] Transform createPos; //공격이 생성되는 position , 플레이어가 부모로 둔 transform을 넣어줘야함
    private float viewRange = 5f; //적 감지 사거리
    private Collider2D nearTarget; //가장 가까운 적을 담기위한 변수
    private float shortest = float.MaxValue; //가장 짧은 거리의 적을 찾기위해 거리 계산한 값을 넣는 변수
    private bool isInsideEnemy = false; //적이 공격 사거리 내에 있나?
    private float nextAttackTime = 0f; 
    private float nextAttackRange = 0.5f; //콤보 공격 간격(쿨타임)


    [Header("특공")]
    [SerializeField] float sp_drvieDecrease = 100f; //특공 드라이브 감소
    [SerializeField] float sp_stayCount = 1f; //특공 경직 시간
    Combo combo;
    void Start()
    {
        combo = new Combo();
    }

    void Update()
    {
        
    }
    public void OnLightAttack(InputValue value) //약공
    {
        if (value.isPressed && Time.time <= nextAttackTime) //키 입력을 받았을 때 한번 실행이 됨
        {
            FindToNearTasrget();
            int attackIndex = combo.currentCount;
            if (isInsideEnemy && nearTarget != null) //적이 공격 사거리, 시야 이내에 있다면
            {
                StartCoroutine(StartLightAttack(attackIndex, nearTarget.gameObject.transform.position));
            }
            else //적이 공격 사거리, 시야 내에 없다면
            {
                StartCoroutine(StartLightAttack(attackIndex, this.transform.right));
            }

            combo.currentCount = (combo.currentCount + 1) % 5;
            nextAttackTime = Time.time + nextAttackRange;
        }
        else if(value.isPressed){
            combo.currentCount = 0;
        }
    }

    //투사체 생성과 투사체에게 정보 넘겨주기
    private IEnumerator StartLightAttack(int index ,Vector2 targetPos)
    {
        yield return new WaitForSeconds(combo.frontDeley[index] /BASE_FPS); //선 딜레이

        Vector2 newPos = createPos.position;
        GameObject obj_lightatk = Instantiate(obj, newPos, Quaternion.identity); //투사체 생성
        OBJ_LightAttack atkInit = obj_lightatk.GetComponent<OBJ_LightAttack>(); //투사체의 스크립트 가져오기
        if (atkInit != null)
        {
            atkInit.Initialize(combo.damage[index], targetPos); //투사체 공격, 가야할 방향(위치) 저장
        }

        yield return new WaitForSeconds(combo.backDeley/BASE_FPS); //후 딜레이
    }


    //overlap의 사거리 내에 있는 적 중에 가장 가까운 적을 찾음
    private void FindToNearTasrget()
    {
        shortest = float.MaxValue; //탐색 할때마다 초기화
        nearTarget = null;
        int count = 0;
      

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, viewRange);
        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                Vector2 targetPos = target.transform.position;
                Vector2 playerPos = transform.position;
                Vector2 dir = (targetPos - playerPos).normalized; //방향
                dir.y = 0;
                Vector2 myForward = transform.right;
                float angle = Vector2.Angle(myForward, dir);
                //시야 이내에 있음
                if (angle < viewAngle * 0.5)
                {
                    count++;
                    //플레이어와 적의 거리를 비교
                    float distance = Vector2.Distance(playerPos, targetPos);
                    //가장 짧은걸 비교
                    if (distance < shortest)
                    {
                        shortest = distance;
                        nearTarget = target;
                    }
                }
            }
        }
        isInsideEnemy = count > 0 ? true : false; // 사거리 내에 시야 내에 적이 있는지 없는지 판별
    }



    public void OnHeavyAttack(InputValue value)//강공
    {

    } 

    public void Abillity(InputValue value) //특공
    {

    }

    
  
}
