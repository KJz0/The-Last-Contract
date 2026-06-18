using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Slider   mainHpSlider;
    [SerializeField] private Slider   ghostHpSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Health Bar Animation")]
    [SerializeField] private float mainSmoothTime  = 0.1f;
    [SerializeField] private float ghostDelay      = 0.5f;
    [SerializeField] private float ghostSmoothTime = 0.3f;

    private int maxHealth;
    private int currentHealth;
    private int attackDamage;

    private float hpTarget        = 0f;
    private float mainVelocity    = 0f;
    private float ghostVelocity   = 0f;
    private float ghostDelayTimer = 0f;

    private readonly List<StatusEffectInstance> activeEffects = new();

    private void Start()
    {
        if (enemyData != null)
        {
            maxHealth    = enemyData.maxHealth;
            attackDamage = enemyData.attackDamage;
            if (nameText != null) nameText.text = enemyData.enemyName;
        }
        else
        {
            maxHealth    = 50;
            attackDamage = 5;
            Debug.LogWarning($"[Enemy] {name}: EnemyData belum di-assign, pakai default stats");
        }

        currentHealth = maxHealth;
        hpTarget      = maxHealth;

        if (mainHpSlider  != null) { mainHpSlider.maxValue  = maxHealth; mainHpSlider.value  = maxHealth; }
        if (ghostHpSlider != null) { ghostHpSlider.maxValue = maxHealth; ghostHpSlider.value = maxHealth; }

        UpdateHealthText();
    }

    private void Update()
    {
        AnimateHealthBars();
    }

    private void AnimateHealthBars()
    {
        if (mainHpSlider != null && !Mathf.Approximately(mainHpSlider.value, hpTarget))
        {
            mainHpSlider.value = Mathf.SmoothDamp(mainHpSlider.value, hpTarget, ref mainVelocity, mainSmoothTime);
            if (Mathf.Abs(mainHpSlider.value - hpTarget) < 0.1f)
            {
                mainHpSlider.value = hpTarget;
                mainVelocity = 0f;
            }
        }

        if (ghostHpSlider != null)
        {
            if (ghostDelayTimer > 0f)
            {
                ghostDelayTimer -= Time.deltaTime;
            }
            else if (!Mathf.Approximately(ghostHpSlider.value, hpTarget))
            {
                ghostHpSlider.value = Mathf.SmoothDamp(ghostHpSlider.value, hpTarget, ref ghostVelocity, ghostSmoothTime);
                if (Mathf.Abs(ghostHpSlider.value - hpTarget) < 0.1f)
                {
                    ghostHpSlider.value = hpTarget;
                    ghostVelocity = 0f;
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // COMBAT
    // ---------------------------------------------------------------

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        int remaining = AbsorbWithShield(amount);
        if (remaining <= 0)
        {
            Debug.Log($"[Enemy] {name}: damage diserap Shield");
            return;
        }

        currentHealth   = Mathf.Clamp(currentHealth - remaining, 0, maxHealth);
        hpTarget        = currentHealth;
        ghostDelayTimer = ghostDelay;

        Debug.Log($"[Enemy] {name} kena {remaining} damage! HP: {currentHealth}/{maxHealth}");
        UpdateHealthText();

        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        hpTarget      = currentHealth;

        // Heal: ghost ikut langsung (tidak perlu delay untuk heal)
        if (ghostHpSlider != null) ghostHpSlider.value = currentHealth;

        UpdateHealthText();
    }

    public void PerformAttack()
    {
        if (PlayerManager.Instance == null) return;
        Debug.Log($"[Enemy] {name} menyerang player {attackDamage} damage");
        PlayerManager.Instance.TakeDamage(attackDamage);
    }

    // ---------------------------------------------------------------
    // STATUS EFFECTS
    // ---------------------------------------------------------------

    public void AddStatusEffect(StatusEffect effect, int duration, int value)
    {
        if (effect == null) return;

        foreach (StatusEffectInstance existing in activeEffects)
        {
            if (existing.effect == effect && effect.TryStack(existing, duration, value))
            {
                Debug.Log($"[Enemy] {name}: {effect.effectName} di-stack");
                return;
            }
        }

        StatusEffectInstance instance = new StatusEffectInstance(effect, duration, value);
        activeEffects.Add(instance);
        effect.OnApply(this, instance);
    }

    public void TickEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffectInstance inst = activeEffects[i];
            inst.effect.OnTick(this, inst);
            inst.duration--;

            if (inst.duration <= 0)
            {
                inst.effect.OnExpire(this, inst);
                activeEffects.RemoveAt(i);
            }
        }
    }

    // ---------------------------------------------------------------
    // PRIVATE
    // ---------------------------------------------------------------

    private int AbsorbWithShield(int incoming)
    {
        int remaining = incoming;
        for (int i = activeEffects.Count - 1; i >= 0 && remaining > 0; i--)
        {
            StatusEffectInstance inst = activeEffects[i];
            if (!(inst.effect is ShieldStatusEffect)) continue;

            if (inst.value >= remaining)
            {
                inst.value -= remaining;
                remaining   = 0;
                if (inst.value <= 0) { inst.effect.OnExpire(this, inst); activeEffects.RemoveAt(i); }
            }
            else
            {
                remaining -= inst.value;
                inst.effect.OnExpire(this, inst);
                activeEffects.RemoveAt(i);
            }
        }
        return remaining;
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = $"{currentHealth}/{maxHealth}";
    }

    private void Die()
    {
        Debug.Log($"[Enemy] {name} mati!");
        TurnManager.Instance?.RemoveEnemy(this);
        gameObject.SetActive(false);
    }
}