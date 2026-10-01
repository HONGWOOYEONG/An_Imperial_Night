/// 작성자 : 유희일
using UnityEngine;

public class Boss_AroundCheck : MonoBehaviour
{
    [Header("접지")]
    [Tooltip("접지 전용 콜라이더. 본체 콜라이더를 넣으면 벽에 붙었을 때도 접지로 잡힌다.")]
    [SerializeField] private BoxCollider2D groundBox;
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("접지로 인정하는 여유 거리. 크게 잡으면 공중에 떠 있어도 접지로 잡힌다.")]
    [SerializeField, Min(0f)] private float groundSkin = 0.05f;

    [Tooltip("스냅을 허용하는 최대 거리. 바닥이 이보다 멀면 스냅하지 않고 낙하로 넘긴다.")]
    [SerializeField, Min(0f)] private float maxSnapDistance = 1f;

    [Header("전방")]
    [SerializeField] private Transform wallCheckOrigin;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField, Min(0f)] private float wallCheckDistance = 0.3f;

    [Header("현재 판정 (읽기 전용)")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isWallAhead;

    public bool IsGrounded => isGrounded;
    public bool IsWallAhead => isWallAhead;

    // 매 물리 프레임마다 배열을 새로 잡지 않도록 결과 버퍼와 필터를 재사용한다.
    private readonly RaycastHit2D[] hits = new RaycastHit2D[1];
    private readonly Collider2D[] overlaps = new Collider2D[1];
    private ContactFilter2D groundFilter;
    private ContactFilter2D wallFilter;

    private void Awake()
    {
        groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundLayer,
            useTriggers = false
        };

        wallFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = wallLayer,
            useTriggers = false
        };

        if (groundBox == null)
        {
            Debug.LogWarning($"{name} : groundBox가 비어 있다. 접지 전용 BoxCollider2D를 인스펙터에 넣어야 접지와 스냅이 동작한다.", this);
        }
    }

    public void FixedTick(int facing)
    {
        isGrounded = CheckGround();
        isWallAhead = CheckWall(facing);
    }

    /// <summary>
    /// 키네마틱 이동이 끝난 뒤 바닥에 붙이기 위한 y축 델타를 구한다.
    /// 위로 올려야 하면 양수, 아래로 내려야 하면 음수다. 값을 실제로 적용하는 것은 Boss_Moter가 한다.
    /// </summary>
    public bool TryGetGroundSnapOffset(out float verticalOffset)
    {
        verticalOffset = 0f;

        if (groundBox == null) return false;

        Bounds box = groundBox.bounds;

        // 바닥을 파고든 채로 이동이 끝나는 경우가 있다. 이때는 아래로 쏘아봐야 거리가 0으로 나와
        // 박힌 채로 남으므로, 겹침을 먼저 확인해서 침투 깊이만큼 위로 밀어낸다.
        int overlapCount = Physics2D.OverlapBox(box.center, box.size, 0f, groundFilter, overlaps);
        if (overlapCount > 0)
        {
            ColliderDistance2D distance = groundBox.Distance(overlaps[0]);

            // 겹쳐 있을 때만 distance가 음수로 나온다. 평지 기준으로 수직으로만 밀어낸다.
            // 경사면까지 정확히 빼내려면 distance.normal로 투영해야 한다.
            if (distance.isValid && distance.distance < 0f)
            {
                verticalOffset = -distance.distance;
                return true;
            }
        }

        // 콜라이더 모양 그대로 아래로 쏜다. Raycast로 재면 박스 중심 기준 거리가 나와서
        // 박스 절반 높이를 손으로 빼야 하고, 인스펙터에서 박스 크기를 바꾸는 순간 그 값이 틀어진다.
        int count = groundBox.Cast(Vector2.down, groundFilter, hits, maxSnapDistance);
        if (count > 0)
        {
            verticalOffset = -hits[0].distance;
            return true;
        }

        // 바닥이 maxSnapDistance보다 멀다. 스냅하지 않고 낙하로 넘긴다.
        return false;
    }

    private bool CheckGround()
    {
        if (groundBox == null) return false;

        // 파고든 상태에서는 Cast가 거리 0을 돌려주므로 겹침을 따로 본다.
        Bounds box = groundBox.bounds;
        if (Physics2D.OverlapBox(box.center, box.size, 0f, groundFilter, overlaps) > 0) return true;

        return groundBox.Cast(Vector2.down, groundFilter, hits, groundSkin) > 0;
    }

    private bool CheckWall(int facing)
    {
        if (wallCheckOrigin == null) return false;

        // facing이 0이면 바라보는 방향이 정해지지 않은 것이므로 검사하지 않는다.
        if (facing == 0) return false;

        return Physics2D.Raycast(wallCheckOrigin.position, new Vector2(facing, 0f), wallFilter, hits, wallCheckDistance) > 0;
    }
}
