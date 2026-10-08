using UnityEngine;
using System.Collections;
public class T_SpecialAtkState : I_TAttackState
{
    [SerializeField] float stayCount = 1f;
    [SerializeField] float frontDelay = 45f;
    [SerializeField] float backDelay = 2f;
    [SerializeField] float atkRange = 5f;
    private float sp_timer = 0f;
    [SerializeField] private float sp_rayTime = 3f;
    private bool sp_hasAttacked = false;
    public void Enter(T_Attack t_Attack)
    {
        t_Attack.StartCoroutine(SpecialAttack(t_Attack));
    }

    public void Exit(T_Attack t_Attack)
    {
        sp_timer = 0f;
        sp_hasAttacked = false;
    }

    public void Update(T_Attack t_Attack)
    {
    }

    private IEnumerator SpecialAttack(T_Attack t_Attack)
    {
        t_Attack.sp_isAttaking = true;
        float normalrgavity = t_Attack.rb.gravityScale;
        if (t_Attack.movement != null)
        {
            t_Attack.movement.AddMovementLock(this);  // 이동 및 점프 제어 비활성화
        }
        t_Attack.movement.SetVelocity(Vector2.zero);
        if (t_Attack.jump.isJumping)
        {
            t_Attack.movement.SetGravityScale(0f);
        }

        yield return new WaitForSeconds(t_Attack.FramesToSeconds(frontDelay));

        sp_timer = 0f;
        float keepRayTime = sp_rayTime / t_Attack.BASE_FPS;
        while (sp_timer < keepRayTime)
        {
            sp_timer += Time.deltaTime;

            Vector2 crtPos = (t_Attack.createPos.position);
            Vector2 attackDir = Vector2.right * t_Attack.movement.FacingDirection;

            RaycastHit2D hit = Physics2D.Raycast(crtPos, attackDir, atkRange);
            Debug.DrawRay(crtPos, Vector2.right * atkRange, Color.yellow, keepRayTime);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Enemy") && !sp_hasAttacked)
                {
                    Debug.Log(hit.collider.name);
                    //공격 처리
                    sp_hasAttacked = true;
                }
            }
            yield return null;
        }
        sp_hasAttacked = false;
        yield return new WaitForSeconds(t_Attack.FramesToSeconds(backDelay));
        if (t_Attack.movement != null)
        {
            t_Attack.movement.ReleaseMovementLock(this); //원래 상태 복구
        }
        t_Attack.movement.SetGravityScale(normalrgavity);
        t_Attack.sp_isAttaking = false;
        t_Attack.playerController?.EndAction(PlayerState.Ability);
    }
}
