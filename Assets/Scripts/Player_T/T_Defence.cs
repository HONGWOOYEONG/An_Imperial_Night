using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class T_Defence : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private T_DriveGauge t_DriveGauge;
    private PlayerController playerController;
    public const float BASE_FPS = 60f;

    [Header("방어")]
    [SerializeField] float d_driveDecease = 0;
    [SerializeField] float d_startDelay = 2f;
    [SerializeField] float d_endDelay = 2f;
    private Coroutine defenceCoroutine = null;

    public bool isDefencing = false;
    private bool isHoldingDefence = false;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        t_DriveGauge = GetComponent<T_DriveGauge>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        playerController.ActionsCancelled += ForceStopDefense;
    }

    public void OnDefence(InputValue value)
    {
        if (!isActiveAndEnabled) return;
        if (value.isPressed)
        {
            if (t_DriveGauge.isBunOut || defenceCoroutine != null) return;
            if (playerController != null && !playerController.TryStartAction(PlayerState.Defending)) return;
            isDefencing = true;
            defenceCoroutine = StartCoroutine(Defence());
        }
        else ForceStopDefense();
    }

    IEnumerator Defence()
    {
        yield return new WaitForSeconds(FrameToSeconds(d_startDelay));
        if (!isDefencing) yield break;
        isHoldingDefence = true;
        playerMovement.SetDefending(true);
        defenceCoroutine = null;
    }

    // 기존 직접 호출은 유지하되, 실제 피격 진입점인 PlayerHealth로 전달합니다.
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        playerHealth?.ReceiveAttack(damageInfo);
    }

    public bool TryDefend(DamageInfo damageInfo)
    {
        // 방어가 성립하면 Health가 HP 피해를 적용하지 않도록 true를 반환합니다.
        if (!isHoldingDefence || damageInfo.damageType == DamageType.UnblockableAttack) return false;
        GuardSuccess(damageInfo);
        return true;
    }

    public void GuardSuccess(DamageInfo damageInfo)
    {
        if (damageInfo.driveDamage > 0f) t_DriveGauge.DecreaseDriveGauge(damageInfo.driveDamage);
    }

    public void GuardFail(DamageInfo damageInfo)
    {
        ForceStopDefense();
        playerHealth.ReceiveAttack(damageInfo);
    }

    public void ForceStopDefense()
    {
        // 번아웃·피격 중단·버튼 해제 모두 판정과 이동 상태를 함께 정리합니다.
        if (defenceCoroutine != null)
        {
            StopCoroutine(defenceCoroutine);
            defenceCoroutine = null;
        }
        isDefencing = false;
        isHoldingDefence = false;
        playerMovement?.SetDefending(false);
        playerController?.EndAction(PlayerState.Defending);
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.ActionsCancelled -= ForceStopDefense;
        }
        ForceStopDefense();
    }

    private float FrameToSeconds(float frame) => frame / BASE_FPS;

}
