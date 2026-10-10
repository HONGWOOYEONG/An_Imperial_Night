using UnityEngine;

public class PlayerPostureUI : MonoBehaviour
{
    [SerializeField] private H_Posture hPosture;
    [SerializeField] private UnityEngine.UI.Image postureSliderL;
    [SerializeField] private UnityEngine.UI.Image postureSliderR;

    private void Awake()
    {
        if (hPosture == null)
            hPosture = GetComponent<H_Posture>();
    }

    private void Start()
    {
        if (hPosture != null)
            UpdatePostureUI(hPosture.CurrentPosture, hPosture.MaxPosture);
    }

    private void OnEnable()
    {
        if (hPosture != null)
            hPosture.OnPostureChanged += UpdatePostureUI;
    }

    private void OnDisable()
    {
        if (hPosture != null)
            hPosture.OnPostureChanged -= UpdatePostureUI;
    }

    public void UpdatePostureUI(float currentPosture, float maxPosture)
    {
        float fillAmount = maxPosture > 0f ? currentPosture / maxPosture : 0f;
        if (postureSliderL != null) postureSliderL.fillAmount = fillAmount;
        if (postureSliderR != null) postureSliderR.fillAmount = fillAmount;
    }
}
