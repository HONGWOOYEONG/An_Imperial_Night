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
        throw new System.NotImplementedException();
    }

    public void Update(T_Attack t_Attack)
    {
        throw new System.NotImplementedException();
    }

    private IEnumerator SpecialAttack(T_Attack t_Attack)
    {
        t_Attack.sp_isAttaking = true;
        float normalrgavity = t_Attack.rb.gravityScale;
        if (t_Attack.movement != null)
        {
            t_Attack.movement.enabled = false;  // 이동 및 점프 제어 비활성화
        }
        t_Attack.rb.linearVelocity = Vector2.zero;
        if (t_Attack.jump.isJumping)
        {
            t_Attack.rb.gravityScale = 0f;
        }

        Vector2 crtPos = (t_Attack.createPos.position);
        yield return new WaitForSeconds(t_Attack.SecondsToFrames(frontDelay));

        sp_timer = 0f;
        float keepRayTime = sp_rayTime / t_Attack.BASE_FPS;
        while (sp_timer < keepRayTime)
        {
            sp_timer += Time.deltaTime;
            RaycastHit2D hit = Physics2D.Raycast(crtPos, Vector2.right, atkRange);
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
        yield return new WaitForSeconds(t_Attack.SecondsToFrames(backDelay));
        if (t_Attack.movement != null)
        {
            t_Attack.movement.enabled = true; //원래 상태 복구
        }
        t_Attack.rb.gravityScale = normalrgavity;
        t_Attack.sp_isAttaking = false;

    }
}
