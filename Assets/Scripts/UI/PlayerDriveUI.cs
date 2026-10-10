using UnityEngine;

public class PlayerDriveUI : MonoBehaviour
{
    [SerializeField] private T_DriveGauge tDriveGauge;
    [SerializeField] private UnityEngine.UI.Image driveSliderL;
    [SerializeField] private UnityEngine.UI.Image driveSliderR;

    private void Awake()
    {
        if (tDriveGauge == null)
            tDriveGauge = GetComponent<T_DriveGauge>();
    }

    private void Start()
    {
        if (tDriveGauge != null)
            UpdateDriveUI(tDriveGauge.DriveGauge, tDriveGauge.MaxDriveGauge);
    }

    private void OnDisable()
    {
        if (tDriveGauge != null)
            tDriveGauge.OnDriveGaugeChanged -= UpdateDriveUI;
    }

    private void OnEnable()
    {
        if (tDriveGauge != null)
            tDriveGauge.OnDriveGaugeChanged += UpdateDriveUI;
    }

    public void UpdateDriveUI(float currentDrive, float maxDrive)
    {
        float fillAmount = maxDrive > 0f ? currentDrive / maxDrive : 0f;
        if (driveSliderL != null) driveSliderL.fillAmount = fillAmount;
        if (driveSliderR != null) driveSliderR.fillAmount = fillAmount;
    }
}
