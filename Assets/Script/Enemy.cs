using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("UI References")]
    [SerializeField] private TMP_Text healthText;

    private readonly List<StatusEffectInstance>
    activeEffects =
        new List<StatusEffectInstance>();

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"[Combat] Musuh kena {damageAmount} damage! HP Sisa: {currentHealth}");
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddStatusEffect(

        StatusEffect effect,
        int duration,
        int value)
    {
        StatusEffectInstance instance =
            new StatusEffectInstance(
                effect,
                duration,
                value);

        activeEffects.Add(instance);

        effect.OnApply(
            this,
            instance);
    }
    public void TickStatusEffects()
    {
        for(int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffectInstance effect =
                activeEffects[i];

            effect.effect.OnTick(
                this,
                effect);

            effect.duration--;

            if(effect.duration <= 0)
            {
                effect.effect.OnExpire(
                    this,
                    effect);

                activeEffects.RemoveAt(i);
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null) healthText.text = $"{currentHealth}/{maxHealth}";
    }

    private void Die()
    {
        Debug.Log("Musuh tewas! Lu menang! 🎉");
        gameObject.SetActive(false);
    }
}