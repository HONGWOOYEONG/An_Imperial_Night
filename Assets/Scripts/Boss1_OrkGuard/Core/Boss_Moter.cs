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

    [Header("현재 상태 (읽기 전용)")]
    [SerializeField] private int facing = 1;

    public int Facing => facing;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveTick()
    {
        Vector3 dir = new (facing,0,0);
        rb.transform.Translate(dir * moveSpeed * Time.deltaTime);
    }

    public void Dash()
    {
        
    }

    public void Teleport(Vector3 point)
    {
        rb.position = point;
    }

    public void Flip()
    {
        facing = facing>0?-1:1;
    }


}
