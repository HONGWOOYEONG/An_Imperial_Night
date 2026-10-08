using System.Drawing;
using UnityEngine;

public class OBJ_HeavyAttack : MonoBehaviour
{
    T_Attack ownerAttack;
    PlayerMovement movement;
    T_DriveGauge t_DriveGauge;
    BoxCollider2D boxCollider = null;
    private DamageInfo heavyDamageInfo;

    [SerializeField] float healthDG = 30f;//회복되는 드라이브 게이지
    [SerializeField] float destroyTime = 7f;
    private float groggyDamage = 10; //기본 groggy 데미지
    private float time = 2.8f; //적중 시간
    private float timer = 0; //적중 시간 체크 
    private float movementTime = 1.7f; //넉백하고나서 움직일 수 있도록 시간체크하는 변수
    private bool isMoveing = false;

    [Header("늘어나는 오브젝트")]
    private Transform playerTransform;
    private Vector2 playerStartPos;
    private Vector3 startScale;
    private Vector3 startPosition;
    private float startWorldWidth;
    private Vector2 direction = Vector2.zero;


    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject tPlayer = GameObject.FindGameObjectWithTag("RangedDealer");
        movement = tPlayer.GetComponent<PlayerMovement>();
        t_DriveGauge = tPlayer.GetComponent<T_DriveGauge>();
        Destroy(this.gameObject, destroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        ExpandOnKnockback();

        if(timer >= movementTime && !isMoveing)
        {
            movement.ReleaseMovementLock(ownerAttack);
            isMoveing = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (timer >= time) return;
        if (collision.gameObject.CompareTag("Enemy"))
        {
            IDamageReceiver receiver = collision.gameObject.GetComponent<IDamageReceiver>();
            if(receiver != null)
            {
                receiver.ReceiveAttack(heavyDamageInfo);
                if (t_DriveGauge != null)
                {
                    t_DriveGauge.HealthSomeOfDriveGauge(healthDG);
                }
                Destroy(gameObject);
            }
          
        }
    }



    //정보 가져올 때 함수
    public void Initialize(T_Attack attack, Vector2 knockbackDir, Transform player , DamageInfo heavyInfo)
    { 
        ownerAttack = attack;
        direction = knockbackDir;
        playerTransform = player;
        playerStartPos = player.position;
        heavyDamageInfo = heavyInfo;

        startScale = transform.localScale;
        startPosition = transform.position;
        startWorldWidth = boxCollider.bounds.size.x;
    }

    //넉백 시 늘어나는 오브젝트 함수
   private void ExpandOnKnockback() 
    {
        // 플레이어나 콜라이더 정보가 없으면 실행하지 않음
        if (playerTransform == null || boxCollider == null)
            return;

        // 플레이어가 넉백 시작 위치에서 실제로 얼마나 이동했는지 계산
        float knockbackDistance =
            Vector2.Distance(playerStartPos, playerTransform.position);

        // 시작 월드 길이에 실제 넉백 거리를 더한 값이 최종 길이
        float targetWorldWidth = startWorldWidth + knockbackDistance;

        // 현재 오브젝트의 시작 월드 길이를 기준으로
        // 최종 scale 비율 계산
        float widthRatio = targetWorldWidth / startWorldWidth;

        // 시작 scale을 기준으로 계산
        Vector3 scale = startScale;
        scale.x = startScale.x * widthRatio;

        transform.localScale = scale;

        // 오브젝트는 중심을 기준으로 커지기 때문에
        // 늘어난 거리의 절반만큼 이동해서 한쪽 끝을 고정
        transform.position = startPosition + new Vector3(direction.x * knockbackDistance * 0.5f, 0f, 0f);

    }


}
