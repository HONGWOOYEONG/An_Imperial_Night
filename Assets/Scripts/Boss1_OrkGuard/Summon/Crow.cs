using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Crow : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private DamageInfo damage = new DamageInfo();

    [Header("Movement")]
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float finalSpeed = 40f;
    [SerializeField] private float accelPower = 5f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Header("Idle")]
    [SerializeField] private Transform centerPoint;
    [SerializeField] private float idleTime = 3f;


    private Animator anim;
    private float timer = 0;

    // public Transform testTarget;
    // public Transform testCenter;

    float moveSpeed = 0;
    float chaseTime;
    Vector2 targetDir;
    bool onChase = false;

    private Transform target;
    private Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > idleTime && !onChase)
            onChase = true;

        if (!onChase)
            targetDir = Check_TargetDir();
    }
    void FixedUpdate()
    {
        if (onDead)
        {
            rb.linearVelocity = targetDir;
            return;
        }

        if (onChase)
        {
            AnimToTarget();
            ChaseMove_Tick();
        }
        else
        {
            FlipToTarget();
            IdleMove_Tick();
        }
    }
    // 까마귀를 생성할 때 호출할 세팅
    // [ContextMenu("test setCrow")]
    // public void Test_SetCrow() => SetCrow(testTarget,testCenter);
    public void SetCrow(Transform target, Transform centerPoint)
    {
        this.target = target;
        this.centerPoint = centerPoint;
        targetDir = Check_TargetDir();
        chaseTime = 0f;
        onChase = false;
    }
    public void FlipToTarget()
    {
        transform.rotation = Quaternion.Euler(0, targetDir.x < 0 ? 180 : 0, 0);
    }

    private void AnimToTarget()
    {
        // 스프라이트는 오른쪽 아래 45도를 가리킴.

        // +45로 보정하지 않으면 머리가 targetDir보다 45도 아래를 향한다.
        // 왼쪽 목표는 Y 180 반전으로 처리한다. z만 돌리면 등이 아래로 가서 뒤집힌 까마귀가 된다.
        bool left = targetDir.x < 0;
        float angle = Mathf.Atan2(targetDir.y, Mathf.Abs(targetDir.x)) * Mathf.Rad2Deg + 45f;
        transform.rotation = Quaternion.Euler(0, left ? 180 : 0, angle);
    }

    Vector2 Check_TargetDir() => (target.position - transform.position).normalized;

    static Vector2 Infinity_Point(Vector2 center, float t, Vector2 size)
        => center + new Vector2(size.x * 0.5f * Mathf.Cos(t), size.y * 0.5f * Mathf.Sin(2f * t));

    void ChaseMove_Tick()
    {
        if (anim != null && anim.GetBool("dive") == false)
            anim.SetBool("dive", true);

        chaseTime += Time.deltaTime;
        moveSpeed = Mathf.Lerp(startSpeed, finalSpeed, speedCurve.Evaluate(chaseTime * accelPower));
        rb.linearVelocity = targetDir * moveSpeed;

        if (chaseTime > 5f)
            Destroy(gameObject);
    }


    private float infTime = 0f;
    private float infSpeed = 2f;
    private Vector2 infSize = new(2, 1);

    void IdleMove_Tick()
    {
        infTime += Time.fixedDeltaTime * infSpeed;
        rb.MovePosition(Infinity_Point(centerPoint.position, infTime, infSize));
    }

    bool onDead = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Untagged")) return; // 카메라 영역과 충돌해서 죽는 경우를 방지하려고 추가
        onDead = true;
        Debug.Log($"충돌대상 : {other.name}");

        var receiver = other.GetComponent<IDamageReceiver>();
        receiver?.ReceiveAttack(damage);
        
        anim.SetTrigger("dead");

        Destroy(gameObject,1f);
    }
}
