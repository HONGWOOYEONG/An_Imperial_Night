/// 작성자 : 유희일
/// 움직임, 위치 변화 등을 담당하는 클래스.
/// 값을 받아서 움직임을 처리하는 역할을 한다.
/// 이동과 관련된 모든 변수와 함수를 책임진다.
/// 
/// 이동
/// 좌우반전
/// 돌진(감속 커브기반)
/// 순간이동

using UnityEngine;

public class Boss_Moter : MonoBehaviour
{
    [Header("이동")]
    [SerializeField, Min(0f)] private float moveSpeed = 3f;

    [Header("돌진")]
    [Tooltip("진행도(0~1)를 이동한 비율로 바꾸는 곡선. 초반이 가파르고 끝이 평평할수록 튀어나갔다가 목적지에서 미끄러지듯 멈춘다.")]
    [SerializeField] private AnimationCurve dashEase = new AnimationCurve(
        // 시작 기울기 3, 끝 기울기 0. 1 - (1-t)^3 과 같은 모양이라 되돌아오거나 목적지를 넘어가지 않는다.
        new Keyframe(0f, 0f, 0f, 3f),
        new Keyframe(1f, 1f, 0f, 0f));

    [Tooltip("돌진 하나가 끝나기까지의 시간. 커브의 x축은 진행도(0~1)이므로 경과 시간을 이 값으로 나눠 넣는다. " +
             "경과 초를 그대로 넣으면 커브의 앞부분만 쓰고 끝나서 감속이 아예 안 나온다.")]
    [SerializeField, Min(0.01f)] private float dashDuration = 0.4f;

    [Header("현재 상태 (읽기 전용)")]
    [SerializeField] private int facing = 1;

    // 돌진은 시작 시점에 출발 위치·방향·거리를 못 박는다. 매 프레임 facing을 다시 읽으면
    // 돌진 도중 Flip이 걸렸을 때 궤적이 공중에서 꺾인다.
    private Vector2 dashOrigin;
    private float dashDistance;
    private int dashFacing;
    private float dashTimer;
    private bool isDashing;

    public int Facing => facing;

    private Rigidbody2D rb;
    private Boss_Controller boss;
    private Animator anim;

    private void Awake()
    {
        boss = GetComponent<Boss_Controller>();
        rb = GetComponent<Rigidbody2D>();
        anim = boss.Anim;
    }

    // Boss_MoveState.FixedTick에서 호출.
    public void Move_FixedTick()
    {
        Set_VelocityX(facing * moveSpeed);
    }

    /// <summary>
    /// 돌진을 시작한다. 출발 위치·방향·거리를 여기서 고정한다.
    /// </summary>
    public void Dash_Begin(float distance)
    {
        dashOrigin = rb.position;
        dashDistance = distance;
        dashFacing = facing;
        dashTimer = 0f;
        isDashing = true;
    }

    /// <summary>
    /// 아직 돌진 중이면 true. 커브가 주는 것은 "지금쯤 있어야 할 위치"지 이동량이 아니다.
    /// 위치를 그대로 대입하면 벽을 뚫으므로, 남은 거리를 이번 스텝의 속도로 바꿔 넘긴다.
    /// 벽에 막히면 rb.position이 안 나가고, 다음 스텝에서 남은 거리가 저절로 다시 계산된다.
    /// </summary>
    public bool Dash_FixedTick()
    {
        if (!isDashing) return false;

        dashTimer += Time.fixedDeltaTime;

        float progress = Mathf.Clamp01(dashTimer / dashDuration);
        float targetX = dashOrigin.x + dashEase.Evaluate(progress) * dashDistance * dashFacing;

        Set_VelocityX((targetX - rb.position.x) / Time.fixedDeltaTime);

        if (progress < 1f) return true;

        isDashing = false;
        Stop_Horizontal();
        return false;
    }

    /// <summary>
    /// 움직이지 않는 상태가 매 물리 스텝에 부른다. 속도 기반이라 이걸 빠뜨리면
    /// 상태가 끝나도 마지막 속도로 계속 미끄러진다.
    /// 돌진 도중 그로기로 끊겨도 여기서 같이 꺼야 한다. isDashing을 남겨두면
    /// Dash_FixedTick을 부르는 상태가 없는데도 돌진 중으로 남아 영원히 미끄러진다.
    /// </summary>
    public void Stop_Horizontal()
    {
        isDashing = false;
        Set_VelocityX(0f);
    }

    // rb의 가로 속도에 쓰는 유일한 지점이다. y는 중력이 쓰는 값이라 절대 건드리지 않는다.
    private void Set_VelocityX(float velocityX)
    {
        rb.linearVelocityX = velocityX;
    }

    public void Teleport(Vector3 point)
    {
        rb.position = point;
    }
    [ContextMenu("텔포")]
    public void Test_Teleport()=> Teleport(Vector3.zero);


    public void HandleFlip()
    {
        if( (boss.Context.BossPosition.x - boss.Context.TargetPosition.x) * facing > 0 )
            Flip();
    }
    public void Flip()
    {
        anim.transform.Rotate(0,180,0);
        facing = facing>0?-1:1;
    }


}
