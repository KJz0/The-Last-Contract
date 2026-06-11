using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// MonoBehaviour enemy. Untuk stats, assign EnemyData di Inspector.
/// Mendukung status effect dinamis: Poison, Burn, Heal, Shield, dll.
/// 
/// BUG FIX: Die() sekarang memanggil TurnManager.RemoveEnemy agar
/// musuh mati tidak terus di-tick dan menyerang.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("UI References")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Slider healthSlider;

    // Runtime stats (diambil dari EnemyData saat Start)
    private int maxHealth;
    private int currentHealth;
    private int attackDamage;

    private readonly List<StatusEffectInstance> activeEffects =
        new List<StatusEffectInstance>();

    // ---------------------------------------------------------------
    // INITIALIZATION
    // ---------------------------------------------------------------

    private void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"[Enemy] {gameObject.name}: EnemyData belum di-assign di Inspector!");
            maxHealth    = 50;
            attackDamage = 5;
        }
        else
        {
            maxHealth    = enemyData.maxHealth;
            attackDamage = enemyData.attackDamage;

            if (nameText != null)
                nameText.text = enemyData.enemyName;

            // Apply passive effects saat spawn jika ada
            if (enemyData.passiveEffectsOnSpawn != null)
            {
                foreach (StatusEffect fx in enemyData.passiveEffectsOnSpawn)
                {
                    if (fx != null)
                        AddStatusEffect(fx, duration: 9999, value: 0);
                }
            }
        }

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // ---------------------------------------------------------------
    // COMBAT
    // ---------------------------------------------------------------

    /// <summary>
    /// Ambil damage masuk. Shield akan menyerap dulu sebelum HP dikurangi.
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (damageAmount <= 0) return;

        // Kurangi Shield terlebih dahulu
        int remaining = AbsorbWithShield(damageAmount);

        if (remaining <= 0)
        {
            Debug.Log($"[Combat] {name}: damage diserap sepenuhnya oleh Shield");
            UpdateHealthUI();
            return;
        }

        currentHealth -= remaining;
        currentHealth  = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[Combat] {name} kena {remaining} damage! HP: {currentHealth}/{maxHealth}");
        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Heal HP enemy. Dipakai oleh HealStatusEffect.
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth += amount;
        currentHealth  = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[Combat] {name} heal {amount} HP! HP: {currentHealth}/{maxHealth}");
        UpdateHealthUI();
    }

    /// <summary>
    /// Enemy menyerang player.
    /// </summary>
    public void PerformAttack()
    {
        if (PlayerManager.Instance == null) return;
        PlayerManager.Instance.TakeDamage(attackDamage);
        Debug.Log($"[Combat] {name} menyerang player {attackDamage} damage");
    }

    // ---------------------------------------------------------------
    // STATUS EFFECTS
    // ---------------------------------------------------------------

    /// <summary>
    /// Tambah atau stack status effect ke enemy ini.
    /// Jika effect sudah ada dan mendukung stacking, TryStack dipanggil.
    /// Jika tidak, instance baru dibuat (stack paralel).
    /// </summary>
    public void AddStatusEffect(StatusEffect effect, int duration, int value)
    {
        if (effect == null)
        {
            Debug.LogWarning($"[Enemy] {name}: AddStatusEffect dipanggil dengan effect null");
            return;
        }

        // Coba stack ke instance yang sudah ada
        foreach (StatusEffectInstance existing in activeEffects)
        {
            if (existing.effect == effect)
            {
                if (effect.TryStack(existing, duration, value))
                {
                    Debug.Log($"[Status] {name}: {effect.effectName} di-stack");
                    return;
                }
            }
        }

        // Tidak bisa di-stack, buat instance baru
        StatusEffectInstance instance = new StatusEffectInstance(effect, duration, value);
        activeEffects.Add(instance);
        effect.OnApply(this, instance);
    }

    /// <summary>
    /// Tick semua status effect aktif. Dipanggil oleh TurnManager setiap akhir turn player.
    /// Iterasi mundur agar aman saat menghapus elemen.
    /// </summary>
    public void TickEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffectInstance instance = activeEffects[i];

            instance.effect.OnTick(this, instance);
            instance.duration--;

            if (instance.duration <= 0)
            {
                instance.effect.OnExpire(this, instance);
                activeEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Ambil total Shield yang sedang aktif (untuk ditampilkan di UI).
    /// </summary>
    public int GetTotalShield()
    {
        int total = 0;
        foreach (StatusEffectInstance inst in activeEffects)
        {
            if (inst.effect is ShieldStatusEffect)
                total += inst.value;
        }
        return total;
    }

    // ---------------------------------------------------------------
    // PRIVATE HELPERS
    // ---------------------------------------------------------------

    /// <summary>
    /// Kurangi Shield instances terlebih dahulu, return sisa damage.
    /// </summary>
    private int AbsorbWithShield(int incomingDamage)
    {
        int remaining = incomingDamage;

        for (int i = activeEffects.Count - 1; i >= 0 && remaining > 0; i--)
        {
            StatusEffectInstance inst = activeEffects[i];
            if (!(inst.effect is ShieldStatusEffect)) continue;

            if (inst.value >= remaining)
            {
                inst.value -= remaining;
                remaining   = 0;

                if (inst.value <= 0)
                {
                    inst.effect.OnExpire(this, inst);
                    activeEffects.RemoveAt(i);
                }
            }
            else
            {
                remaining  -= inst.value;
                inst.effect.OnExpire(this, inst);
                activeEffects.RemoveAt(i);
            }
        }

        return remaining;
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = $"{currentHealth}/{maxHealth}";

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value    = currentHealth;
        }
    }

    /// <summary>
    /// BUG FIX: Dihapus dari TurnManager saat mati agar tidak terus di-tick/menyerang.
    /// </summary>
    private void Die()
    {
        Debug.Log($"[Combat] {name} mati!");

        // Hapus dari daftar aktif TurnManager
        if (TurnManager.Instance != null)
            TurnManager.Instance.RemoveEnemy(this);

        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------
    // EDITOR DEBUG
    // ---------------------------------------------------------------

    [ContextMenu("Debug Status Effects")]
    private void DebugStatusEffects()
    {
        if (activeEffects.Count == 0)
        {
            Debug.Log($"[{name}] Tidak ada status effect aktif");
            return;
        }

        foreach (StatusEffectInstance inst in activeEffects)
        {
            Debug.Log($"[{name}] {inst.effect.effectName}: value={inst.value}, duration={inst.duration}");
        }
    }
}