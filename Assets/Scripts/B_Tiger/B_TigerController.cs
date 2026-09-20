using UnityEngine;

public class B_TigerController : MonoBehaviour
{
    private B_TigerUtilityAI utilityAI;
    private B_TigerFSM tigerFSM;
    private Rigidbody2D rb;
    private Animator animator;


    [Header("Status")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxPosture = 100f;
    [SerializeField] private float currentPosture;
    [SerializeField] private float moveSpeed = 5f;

    private float facingDirection = 1f;
    public float FacingDirection => facingDirection;
    private bool isGroggy;
    public bool IsGroggy => isGroggy;
    public Rigidbody2D Rb => rb;
    public B_TigerFSM FSM => tigerFSM;
    public Animator Animator => animator;

    private void Awake()
    {
        utilityAI = GetComponent<B_TigerUtilityAI>();
        tigerFSM = GetComponent<B_TigerFSM>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        currentPosture = 0f;
        isGroggy = false;
    }

    private void Update()
    {
        if (currentPosture >= maxPosture && !isGroggy)
        {
            isGroggy = true;
            tigerFSM.ChangeState(tigerFSM.GroggyState);
        }
    }

    public void MoveTowartTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - rb.position).normalized;

        if (direction.sqrMagnitude <= 0f)
        {
            Stop();
            return;
        }

        RotationToTarget(direction);

        Vector2 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    public void RotationToTarget(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) <= 0.01f)
            return;

        facingDirection = Mathf.Sign(direction.x);

        Vector3 scale = transform.localScale;
        // 호랑이 원본 스프라이트가 왼쪽을 바라보고 있으므로 화면 반전만 반대로 적용합니다.
        // facingDirection 자체는 이동, 공격, 반동 계산에 사용하는 월드 방향을 유지합니다.
        scale.x = Mathf.Abs(scale.x) * -facingDirection;
        transform.localScale = scale;   
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    // 패턴은 UtilityAI를 직접 참조하지 않고 컨트롤러를 통해 타겟을 요청합니다.
    public PlayerContext GetNearestTarget()
    {
        return utilityAI != null ? utilityAI.GetNearestTarget() : null;
    }

    public PlayerContext GetNearestTarget(Vector2 origin)
    {
        return utilityAI != null ? utilityAI.GetNearestTarget(origin) : null;
    }

    public PlayerContext GetFarthestTarget()
    {
        return utilityAI != null ? utilityAI.GetFarthestTarget() : null;
    }

    // 위치변환 플래그를 읽고 초기화하는 책임도 컨트롤러를 경유합니다.
    public bool ConsumeTargetTransPosition(PlayerContext target)
    {
        return target != null && target.ConsumeTransPosition();
    }

    public Vector2 GetTargetPosition(PlayerContext target)
    {
        return target != null ? target.getPosition() : rb.position;
    }

    public void EndGroggy()
    {
        currentPosture = maxPosture / 2;
        isGroggy = false;
    }
}
