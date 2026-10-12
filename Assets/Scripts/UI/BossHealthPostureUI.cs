using System;
using UnityEngine;

public class BossHealthPostureUI : MonoBehaviour
{
    [SerializeField] private Boss_Health bossHP;
    [SerializeField] private UnityEngine.UI.Image postureSliderL;
    [SerializeField] private UnityEngine.UI.Image postureSliderR;
    [SerializeField] private UnityEngine.UI.Image healthSlider;

    private void Awake()
    {
        if (bossHP == null)
            bossHP = GetComponent<Boss_Health>();
    }

    private void Start()
    {
        UpdateHPUI(bossHP.CurrentHp, bossHP.MaxHp);
        UpdatePostureUI(bossHP.CurrentBalance, bossHP.MaxBalance);
    }

    private void OnEnable()
    {
        if (bossHP != null)
        {
            bossHP.OnHpChanged += UpdateHPUI;
            bossHP.OnBalanceChanged += UpdatePostureUI;
        }
    }

    private void OnDisable()
    {
        if (bossHP != null)
        {
            bossHP.OnHpChanged -= UpdateHPUI;
            bossHP.OnBalanceChanged -= UpdatePostureUI;
        }
    }

    public void UpdatePostureUI(float currentPosture, float maxPosture)
    {
        float fillAmount = maxPosture > 0f ? currentPosture / maxPosture : 0f;

        if (postureSliderL != null) postureSliderL.fillAmount = fillAmount;
        if (postureSliderR != null) postureSliderR.fillAmount = fillAmount;
    }

    public void UpdateHPUI(float currentHP, float maxHP)
    {
        float fillAmount = maxHP > 0f ? currentHP / maxHP : 0f;

        if (healthSlider != null) healthSlider.fillAmount = fillAmount;
    }
}
