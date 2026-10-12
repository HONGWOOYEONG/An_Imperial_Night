using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineCamera fieldVCam;
    [SerializeField] private CinemachineCamera bossVCam;

    [Header("Priority")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // �� ���� �� �⺻�� �ʵ� ī�޶�
        SetFieldCamera();
    }

    public void SetFieldCamera()
    {
        fieldVCam.Priority.Value = activePriority;
        bossVCam.Priority.Value = inactivePriority;
    }

    public void SetBossCamera()
    {
        Debug.Log($"���� �� - field:{fieldVCam.Priority.Value}, boss:{bossVCam.Priority.Value}");
        bossVCam.Priority.Value = activePriority;
        fieldVCam.Priority.Value = inactivePriority;
    }

    public IEnumerator PlayBossIntro(Transform bossTarget, float duration, float zoomSize)
    {
        if (bossTarget == null || bossVCam == null)
            yield break;

        Transform previousTarget = bossVCam.Target.TrackingTarget;
        float previousSize = bossVCam.Lens.OrthographicSize;

        bossVCam.Target.TrackingTarget = bossTarget;
        bossVCam.Lens.OrthographicSize = Mathf.Max(0.1f, zoomSize);
        SetBossCamera();

        yield return new WaitForSeconds(Mathf.Max(0f, duration));

        bossVCam.Target.TrackingTarget = previousTarget;
        bossVCam.Lens.OrthographicSize = previousSize;

        // CM_Cam2 remains the active Map 2 gameplay camera after the intro.
        bossVCam.Priority.Value = activePriority;
        fieldVCam.Priority.Value = inactivePriority;
    }
}
