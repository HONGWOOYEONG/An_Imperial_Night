using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class H_Posture : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerHealth playerHealth;
    private H_Defence hDef;
    private PlayerMovement playerMovement;

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

    private T_Defence tDef;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        hDef = GetComponent<H_Defence>();
    }

    // Update is called once per frame
    private float FrameToSeconds(int frame)
    {
        return frame / BASE_FPS;
    }

    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (playerMovement.IsDashing)
        {
            Debug.Log("대시무적 회피");
            return;
        }

        if (hDef.IsParrying)
        {
            Debug.Log("패링 성공");
            tDef.driveGauge += parryOnDrive;
            if(tDef.driveGauge > tDef.dg_max) tDef.driveGauge = tDef.dg_max;
            return;
        }

        if (hDef.IsDefending)
        {
            Debug.Log("방어 성공");

            currentPosture += damageInfo.postureDamage;

            if (currentPosture >= maxPosture && !isGroggy)
            {
                if (RegenPosture != null)
                {
                    StopCoroutine(RegenPosture);
                    RegenPosture = null;
                }

                StartCoroutine(StartGroggy());
            }
            else
            {
                RestartRegenPosture();
            }

            return;
        }

        playerHealth.DamagedFromAtk(damageInfo);
        currentPosture += damageInfo.postureDamage;
    }

    private void RestartRegenPosture()
    {
        if (RegenPosture != null)
        {
            StopCoroutine(RegenPosture);
        }

        RegenPosture = StartCoroutine(StartRegenPosture());
    }

    IEnumerator StartRegenPosture()
    {
        yield return new WaitForSeconds(FrameToSeconds(postureRegenTime));

        postureRegenPercent = playerHealth.CurrentHP / playerHealth.MaxHP;

        while (true)
        {
            currentPosture -= postureRegenPercent * postureRegenAmount * Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();

            if (currentPosture <= 0)
            {
                currentPosture = 0;
                break;
            }
        }

        RegenPosture = null;
    }

    IEnumerator StartGroggy()
    {
        isGroggy = true;

        if (RegenPosture != null)
        {
            StopCoroutine(RegenPosture);
            RegenPosture = null;
        }

        playerMovement.ResetControlState();

        playerInput.DeactivateInput(); // 입력 무시
        yield return new WaitForSeconds(FrameToSeconds(postureGroggy));

        isGroggy = false;
        currentPosture = 0f;

        if (!playerHealth.IsDead)
        {
            playerInput.ActivateInput();
        }
    }
}