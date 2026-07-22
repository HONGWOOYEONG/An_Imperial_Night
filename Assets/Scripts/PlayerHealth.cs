using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    private PlayerInput playerInput;
    private Coroutine stunCoroutine;
    Rigidbody2D rb;
    [Header("Health")]
    [SerializeField] private float maxHP = 1000;
    private float currentHP;

    private bool isDead = false;
    public bool IsDead => isDead;
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;
    }

    // Update is called once per frame
    void Death()
    {
        playerInput.DeactivateInput(); //입력 무시
    }

    public void DamagedFromAtk(DamageInfo damageInfo)
    {
        if (isDead) return;

        currentHP -= damageInfo.damage;
        if (currentHP <= 0)
        {
            isDead = true;
            Death();
            return;
        }

        rb.AddForce(damageInfo.damageDir*damageInfo.knockbackPower, ForceMode2D.Impulse);
        
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(DamageStun(damageInfo.stunTime));
    } // 적 FSM을 작성할 때, 호출하는 함수

    IEnumerator DamageStun(float damagedStun)
    {
        playerInput.DeactivateInput(); //입력 무시
        Debug.Log("피격으로인한 경직"); //추후 애니메이션 + FSM 연결할 때 사용
        yield return new WaitForSeconds(damagedStun);

        if(!isDead) playerInput.ActivateInput();
        stunCoroutine = null;
    }
}
