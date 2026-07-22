using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class H_Defence : MonoBehaviour
{
    [Header("Defence")]
    [SerializeField] private int defStartupTime = 2;
    [SerializeField] private int defRecoveryTime = 5;
    [SerializeField] private int parryDurationTime = 30;

    private bool isDefending = false;
    private bool isParrying = false;

    private Coroutine defenceCoroutine;

    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    public const float BASE_FPS = 60;

    private bool isAbilityParrying = false;
    private bool isAbilityDefending = false;

    public bool IsParrying => isParrying || isAbilityParrying;
    public bool IsDefending => isDefending || isAbilityDefending;
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private float FrameToSeconds(int frame)
    {
        return frame / BASE_FPS;
    }

    public void OnDefence(InputValue value)
    {
        if (value.isPressed)
        {
            if (defenceCoroutine != null) StopCoroutine(defenceCoroutine);

            defenceCoroutine = StartCoroutine(StartDefence());
        }
        else
        {
            if (defenceCoroutine != null)
            {
                StopCoroutine(defenceCoroutine);
                defenceCoroutine = null;
            }
            isDefending = false;
            isParrying = false;

            playerMovement.SetDefending(false);
        }
    }

    IEnumerator StartDefence()
    {
        yield return new WaitForSeconds(FrameToSeconds(defStartupTime));

        isDefending = true;
        isParrying = true;

        playerMovement.SetDefending(true);

        yield return new WaitForSeconds(FrameToSeconds(parryDurationTime));

        isParrying = false;
        defenceCoroutine = null;
    }

    public void StartAbilityParry()
    {
        isAbilityParrying = true;
    }

    public void EndAbilityParry()
    {
        isAbilityParrying = false;
    }

    public void StartAbilityDefence()
    {
        isAbilityDefending = true;
    }

    public void EndAbilityDefence()
    {
        isAbilityDefending = false;
    }

}
