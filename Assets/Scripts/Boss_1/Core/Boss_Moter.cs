/// 작성자 : 유희일
/// 움직임, 위치 변화 등을 담당하는 클래스.
/// 값을 받아서 움직임을 처리하는 역할을 한다.
/// 이동과 관련된 모든 변수와 함수를 책임진다.
/// 
/// 이동
/// 좌우반전
/// 점프(애니메이션 커브기반)
/// 순간이동
/// 지면스냅

using UnityEngine;

public class Boss_Moter : MonoBehaviour
{
    // 좌우가 거의 겹쳤을 때 미세한 x 차이로 방향이 매 프레임 뒤집히는 것을 막는다.
    private const float FaceDeadZone = 0.05f;

    [Header("이동")]
    [SerializeField, Min(0f)] private float moveSpeed = 3f;

    [Header("점프")]
    [SerializeField] private AnimationCurve jumpCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f));

    [Header("현재 상태 (읽기 전용)")]
    [SerializeField] private int facing = 1;
    [SerializeField] private bool isKinematicMoving;

    public int Facing => facing;
    public bool IsKinematicMoving => isKinematicMoving;

    private Rigidbody2D rb;

    // 상태가 Update에서 넘겨준 이동 방향. 실제 적용은 FixedTick에서 한다.
    private int moveInput;
    private Boss_AroundCheck aroundCheck;


    private Vector2 moveStartPosition;
    private float moveElapsed;
    private float moveDuration;
    private float moveHeight;
    private float moveDistance;
    private int moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // controller에게 aroundcheck를 주입받음.
    public void SetAroundCheck(Boss_AroundCheck check)
    {
        aroundCheck = check;
    }

    public void FixedTick()
    {
        if (isKinematicMoving)
        {
            TickKinematicMove();
            return;
        }

        Apply_Move();
    }

    // 방향만 받아둔다. 상태의 Tick은 Update에서 도는데 위치를 옮기는 것은 물리 주기의 일이라,
    // 여기서 바로 옮기면 프레임 수에 따라 이동 거리가 달라진다.
    public void Move(int direction)
    {
        moveInput = direction;
    }

    public void Stop()
    {
        moveInput = 0;

        // y는 건드리지 않는다. 0으로 만들면 낙하 중에 공중에서 한 번 멈칫한다.
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void Flip(int direction)
    {
        if (direction == 0) return;
        if (facing == direction) return;

        facing = direction;
        transform.rotation = Quaternion.Euler(0f, facing == 1 ? 0f : 180f, 0f);
    }

    public void FacePlayer(Vector2 targetPosition)
    {
        float toTarget = targetPosition.x - rb.position.x;

        if (Mathf.Abs(toTarget) < FaceDeadZone) return;

        Flip(toTarget > 0f ? 1 : -1);
    }

    public void Jump(float height, float distance, float speed)
    {
        if (speed <= 0f)
        {
            Debug.LogWarning($"{name} : Jump의 speed가 0 이하다. 패턴 데이터의 속도값을 0보다 크게 넣어야 한다.", this);
            return;
        }

        moveStartPosition = rb.position;
        moveHeight = height;
        moveDistance = distance;
        moveDirection = facing;
        moveElapsed = 0f;

        // 제자리 점프는 distance가 0이라 거리로는 시간을 못 구한다.
        // 올라갔다 내려오는 총 이동량을 height * 2로 보고 시간을 낸다.
        moveDuration = distance > 0f ? distance / speed : height * 2f / speed;

        BeginKinematicMove();
    }

    public void Teleport(Vector2 position)
    {
        CancelKinematicMove();

        rb.position = position;
        rb.linearVelocity = Vector2.zero;

        SnapToGround();
    }

    /// <summary>
    /// 접지 박스와 바닥 표면 사이의 거리를 재서 그만큼 옮긴다.
    /// 거리를 재는 것은 Boss_AroundCheck가, 옮기는 것은 rb를 소유한 이 클래스가 한다.
    /// </summary>
    public void SnapToGround()
    {
        if (aroundCheck == null) return;

        if (!aroundCheck.TryGetGroundSnapOffset(out float verticalOffset)) return;

        rb.position += new Vector2(0f, verticalOffset);
    }

    /// <summary>
    /// 그로기나 사망. 끊고 나면 다시 중력을 받는다.
    /// </summary>
    public void CancelKinematicMove()
    {
        if (!isKinematicMoving) return;

        isKinematicMoving = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
    }

    private void Apply_Move()
    {
        if (moveInput == 0) return;

        if (aroundCheck != null)
        {
            // 벽에 붙은 채로 계속 밀면 콜라이더가 벽을 파고든다.
            if (aroundCheck.IsWallAhead && moveInput == facing) return;

            // 공중에서는 옮기지 않는다. MovePosition은 그 스텝의 y까지 확정하므로
            // 공중에서 부르면 중력이 무시되고 보스가 그 높이에 떠 버린다.
            if (!aroundCheck.IsGrounded) return;
        }

        float delta = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + new Vector2(delta, 0f));
    }

    private void TickKinematicMove()
    {
        moveElapsed += Time.fixedDeltaTime;

        float progress = moveDuration <= 0f ? 1f : Mathf.Clamp01(moveElapsed / moveDuration);

        float x = moveStartPosition.x + moveDirection * moveDistance * progress;
        float y = moveStartPosition.y + jumpCurve.Evaluate(progress) * moveHeight;

        if (progress < 1f)
        {
            rb.MovePosition(new Vector2(x, y));
            return;
        }
        rb.position = new Vector2(x, y);

        SnapToGround();
        CancelKinematicMove();
    }

    private void BeginKinematicMove()
    {
        isKinematicMoving = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
    }
}
