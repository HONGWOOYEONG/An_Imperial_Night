using System.Collections;
using UnityEngine;

public class H_Posture : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private H_Defence hDef;
    private PlayerMovement playerMovement;
    private PlayerController playerController;

    public const float BASE_FPS = 60;
    private bool isGroggy = false;
    private Coroutine RegenPosture;

    [Header("MeleeORIGINAL")]
    [SerializeField] private float maxPosture = 1000f;
    [SerializeField] private float currentPosture = 0;
    [SerializeField] private int postureGroggy;
    [SerializeField] private int postureRegenTime = 50;
    [SerializeField] private float postureRegenAmount = 100;
    private float postureRegenPercent;

    [Header("Parry")]
    [SerializeField] private float parryOnDrive = 70.0f;
    private T_DriveGauge tDriveGauge;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        playerController = GetComponent<PlayerController>();
        hDef = GetComponent<H_Defence>();
        GameObject tPlayer = GameObject.FindGameObjectWithTag("RangedDealer");
        if (tPlayer != null) tDriveGauge = tPlayer.GetComponent<T_DriveGauge>();
    }

    private float FrameToSeconds(int frame) => frame / BASE_FPS;

    // 기존 직접 호출은 유지하되, 실제 피격 진입점인 PlayerHealth로 전달합니다.
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        playerHealth?.ReceiveAttack(damageInfo);
    }

    public bool TryDefend(DamageInfo damageInfo)
    {
        if (playerMovement.IsDashing) return true;
        // damageDir은 공격자에서 피격자로 향하므로 반대 방향이 공격자가 있는 쪽입니다.
        bool fromFront = Vector2.Dot(
            new Vector2(playerMovement.FacingDirection, 0), -damageInfo.damageDir) > 0;
        if (!fromFront) return false;

        if (hDef.IsParrying)
        {
            tDriveGauge?.HealthSomeOfDriveGauge(parryOnDrive);
            return true;
        }

        if (hDef.IsDefending && damageInfo.damageType != DamageType.UnblockableAttack)
        {
            ApplyPostureDamage(damageInfo.postureDamage);
            return true;
        }
        return false;
    }

    public void ApplyPostureDamage(float amount)
    {
        // 방어 중 받은 피해와 일반 피격 모두 같은 자세/그로기 규칙을 사용합니다.
        if (isGroggy || amount <= 0f) return;
        currentPosture = Mathf.Min(maxPosture, currentPosture + amount);
        if (currentPosture >= maxPosture)
        {
            if (RegenPosture != null)
            {
                StopCoroutine(RegenPosture);
                RegenPosture = null;
            }
            StartCoroutine(StartGroggy());
        }
        else RestartRegenPosture();
    }

    private void RestartRegenPosture()
    {
        if (RegenPosture != null) StopCoroutine(RegenPosture);
        RegenPosture = StartCoroutine(StartRegenPosture());
    }

    private IEnumerator StartRegenPosture()
    {
        yield return new WaitForSeconds(FrameToSeconds(postureRegenTime));
        postureRegenPercent = playerHealth.CurrentHP / playerHealth.MaxHP;
        while (currentPosture > 0f)
        {
            currentPosture = Mathf.Max(0f,
                currentPosture - postureRegenPercent * postureRegenAmount * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        RegenPosture = null;
    }

    private IEnumerator StartGroggy()
    {
        // 입력 제한은 컨트롤러에 맡기고, 자세 수치와 지속 시간만 여기서 관리합니다.
        isGroggy = true;
        playerController?.BeginGroggy();
        yield return new WaitForSeconds(FrameToSeconds(postureGroggy));
        currentPosture = 0f;
        isGroggy = false;
        playerController?.EndGroggy();
    }

    public void ResetPosture()
    {
        StopAllCoroutines();
        RegenPosture = null;
        currentPosture = 0f;
        isGroggy = false;
        playerController?.EndGroggy();
    }

    private void OnDisable() => ResetPosture();
}
