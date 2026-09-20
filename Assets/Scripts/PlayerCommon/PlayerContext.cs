using System.Collections.Generic;
using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    private Transform thisTransform;
    private Vector3 currentPosition;
    private bool doTransPosition;

    private void Awake()
    {
        thisTransform = GetComponent<Transform>();
        
    }

    private void Update()
    {
        currentPosition = transform.position;
    }

    public Vector2 getPosition()
    {
        return currentPosition;
    }

    public void setTransPosition()
    {
        doTransPosition = true;
    }

    // 위치변환 신호를 한 번 읽은 뒤 false로 되돌립니다.
    // bool을 계속 true로 두면 다음 패턴에서도 과거의 위치변환을 감지하게 됩니다.
    public bool ConsumeTransPosition()
    {
        if (!doTransPosition)
        {
            return false;
        }

        doTransPosition = false;
        return true;
    }
}
