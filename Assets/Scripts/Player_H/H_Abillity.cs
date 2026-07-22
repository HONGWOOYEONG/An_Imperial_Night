using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class H_Abillity : MonoBehaviour
{
    private Rigidbody2D rb;
    private H_Defence hDef;

    [Header("PositionSwap")]
    [SerializeField] private float avillityCooldown = 2f;
    [SerializeField] private int avillityStartupTime = 8;
    [SerializeField] private int avillityParryingTime = 10;
    [SerializeField] private int avillityDefenceTime = 20;

    private float nextAvillityTime;

    [SerializeField] private Rigidbody2D target;

    public const float BASE_FPS = 60;

    private bool isAbilityInvincible = false;
    private bool isUsingAbility = false;

    public bool IsAbilityInvincible => isAbilityInvincible;
    public bool IsUsingAbility => isUsingAbility;

    private float FrameToSeconds(int frame)
    {
        return frame / BASE_FPS;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hDef = GetComponent<H_Defence>();
    }

    public void OnAbility(InputValue value)
    {
        if (!value.isPressed) return;
        if (Time.time < nextAvillityTime) return;
        if (isUsingAbility) return;
        if (target == null) return;

        StartCoroutine(StartAvillity());
    }

    IEnumerator StartAvillity()
    {
        isUsingAbility = true;
        nextAvillityTime = Time.time + avillityCooldown;

        // 위치 교환 전 시전 구간
        // 이 시간 동안에는 공격을 완전히 무시
        isAbilityInvincible = true;

        yield return new WaitForSeconds(
            FrameToSeconds(avillityStartupTime)
        );

        Vector2 myPosition = rb.position;
        Vector2 targetPosition = target.position;

        rb.position = targetPosition;
        target.position = myPosition;

        float tempRotation = rb.rotation;

        rb.rotation = target.rotation;
        target.rotation = tempRotation;

        rb.linearVelocity = Vector2.zero;
        target.linearVelocity = Vector2.zero;

        isAbilityInvincible = false;

        hDef.StartAbilityParry();

        yield return new WaitForSeconds(FrameToSeconds(avillityParryingTime));

        hDef.EndAbilityParry();

        hDef.StartAbilityDefence();

        yield return new WaitForSeconds(FrameToSeconds(avillityDefenceTime));

        hDef.EndAbilityDefence();

        isUsingAbility = false;
    }
}