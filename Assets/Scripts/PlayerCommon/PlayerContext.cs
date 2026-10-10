using System.Collections;
using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    public Transform trans;
    private Vector3 currentPosition;
    private bool doTransPosition;
    private Coroutine markCoroutine;
    public bool IsMarked { get; private set; }
    private PlayerMovement playerMovement;

    public int FacingDirection => playerMovement != null ? playerMovement.FacingDirection : 1;

    public void ApplyMark(float duration)
    {
        if (markCoroutine != null) StopCoroutine(markCoroutine);
        IsMarked = duration > 0f;
        markCoroutine = IsMarked ? StartCoroutine(ExpireMark(duration)) : null;
    }

    private IEnumerator ExpireMark(float duration)
    {
        yield return new WaitForSeconds(duration);
        IsMarked = false;
        markCoroutine = null;
    }

    public void ClearMark()
    {
        if (markCoroutine != null) StopCoroutine(markCoroutine);
        markCoroutine = null;
        IsMarked = false;
    }

    private void OnDisable() => ClearMark();

    private void Awake()
    {
        trans = transform;
        playerMovement = GetComponent<PlayerMovement>();
        currentPosition = trans.position;
    }

    private void Update()
    {
        currentPosition = trans.position;
    }

    public Vector2 getPosition()
    {
        return currentPosition;
    }

    public void setTransPosition()
    {
        currentPosition = trans.position;
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
