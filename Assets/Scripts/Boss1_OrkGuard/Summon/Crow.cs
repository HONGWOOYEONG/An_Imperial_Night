using System.Threading;
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

    private float timer = 0;

    // public Transform testTarget;
    // public Transform testCenter;

    float moveSpeed = 0;
    float chaseTime;
    Vector2 moveDir;
    Vector2 targetDir;
    bool onChase = false;

    private Transform target;
    private Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > idleTime && !onChase)
            onChase = true;
        if (!onChase)
            targetDir = Check_Target();
    }
    void FixedUpdate()
    {
        if (onChase)
            ChaseMove_Tick();
        else
            IdleMove_Tick();
    }
    // 까마귀를 생성할 때 호출할 세팅
    // [ContextMenu("test setCrow")]
    // public void Test_SetCrow() => SetCrow(testTarget,testCenter);
    public void SetCrow(Transform target,Transform centerPoint)
    {
        this.target = target;
        this.centerPoint = centerPoint;
        targetDir = Check_Target();
        chaseTime = 0f;
        onChase = false;
    }

    Vector2 Check_Target() => (target.position - transform.position).normalized;

    static Vector2 Infinity_Point(Vector2 center, float t, Vector2 size)
        => center + new Vector2(size.x * 0.5f * Mathf.Cos(t), size.y * 0.5f * Mathf.Sin(2f * t));

    void ChaseMove_Tick()
    {
        chaseTime += Time.deltaTime;
        moveSpeed = Mathf.Lerp(startSpeed, finalSpeed, speedCurve.Evaluate(chaseTime * accelPower));
        rb.linearVelocity = targetDir * moveSpeed;
    
    }
    private float infTime = 0f;
    private float infSpeed = 2f;
    private Vector2 infSize = new (2,1);

    void IdleMove_Tick()
    {
        infTime += Time.fixedDeltaTime * infSpeed;
        rb.MovePosition(Infinity_Point(centerPoint.position, infTime, infSize));
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.CompareTag("Enemy")) return;
        
        Debug.Log($"충돌대상 : {other.name}");

        var receiver = other.GetComponent<IDamageReceiver>();
        receiver?.ReceiveAttack(damage);

        Destroy(gameObject);
    }
}
