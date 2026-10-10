using System;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private UnityEngine.UI.Image healthSlider;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        if (playerHealth != null)
            UpdateHealthUI(playerHealth.CurrentHP, playerHealth.MaxHP);
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;
    }

    public void UpdateHealthUI(float currentHP, float maxHP)
    {
        if (healthSlider != null)
            healthSlider.fillAmount = maxHP > 0f ? currentHP / maxHP : 0f;
    }

}
