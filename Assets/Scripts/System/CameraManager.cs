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

        // 씬 시작 시 기본은 필드 카메라
        SetFieldCamera();
    }

    public void SetFieldCamera()
    {
        fieldVCam.Priority.Value = activePriority;
        bossVCam.Priority.Value = inactivePriority;
    }

    public void SetBossCamera()
    {
        Debug.Log($"변경 후 - field:{fieldVCam.Priority.Value}, boss:{bossVCam.Priority.Value}");
        bossVCam.Priority.Value = activePriority;
        fieldVCam.Priority.Value = inactivePriority;
    }
}