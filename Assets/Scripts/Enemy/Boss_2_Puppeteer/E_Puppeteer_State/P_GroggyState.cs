using Unity.VisualScripting;
using UnityEngine;

//그로기 상태
public class P_GroggyState : IPuppeteerState
{
    [SerializeField] float groggyTime = 5f; //그로기 지속 시간
    [SerializeField] float gravity = 2f; //그로기 상태일 때 원래의 중력에 곱해질 값
    Rigidbody2D rb;
    float nextStateTime = 0;
    float normalGravity;
    public void Enter(E_PuppeteerController controller)
    {
        Debug.Log("그로기 상태 진입");
        rb = controller.AddComponent<Rigidbody2D>();

        nextStateTime = Time.time + groggyTime;

        normalGravity = rb.gravityScale;       
        rb.gravityScale = normalGravity * gravity; //그로기 상태일 때 중력값이 크게 작용
    }

    public void Exit(E_PuppeteerController controller)
    {       
        Debug.Log("그로기 상태 해제");
    }

    public void Update(E_PuppeteerController controller)
    {
        if (Time.time >= nextStateTime)
        {
            nextStateTime = 0f;
            rb.gravityScale = normalGravity; //원래 중력 값으로 복귀
            controller.ChangeState(controller.idle);
        }
    }
}
