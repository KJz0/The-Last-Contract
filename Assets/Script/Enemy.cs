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

    private Dictionary<string, int> activeStatusEffects = new Dictionary<string, int>();

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

    public void ApplyStatusEffect(string effectName, int duration)
    {
        if (activeStatusEffects.ContainsKey(effectName))
        {
            activeStatusEffects[effectName] = duration;
        }
        else
        {
            activeStatusEffects.Add(effectName, duration);
        }

        Debug.Log($"[Effect] Musuh terkena efek {effectName} selama {duration} turn!");
    }

    public void UpdateStatusEffects()
    {
        List<string> effectsToRemove = new List<string>();
        List<string> activeEffects = new List<string>(activeStatusEffects.Keys);

        foreach (var effect in activeEffects)
        {
            activeStatusEffects[effect]--;

            if (activeStatusEffects[effect] <= 0)
            {
                effectsToRemove.Add(effect);
            }
            else
            {
                ApplyStatusDamage(effect);
            }
        }

        foreach (var effect in effectsToRemove)
        {
            activeStatusEffects.Remove(effect);
            Debug.Log($"[Effect] Efek {effect} hilang dari musuh");
        }
    }

    private void ApplyStatusDamage(string effectName)
    {
        switch (effectName)
        {
            case "Poison":
                break;
            case "Burn":
                break;
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