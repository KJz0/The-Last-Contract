using UnityEngine;

/// <summary>
/// Abstract base untuk semua status effect.
/// Buat ScriptableObject baru yang inherit class ini untuk effect baru
/// (Poison, Burn, Heal, Shield, Stun, dll).
/// </summary>
public abstract class StatusEffect : ScriptableObject
{
    [Header("Info")]
    public string effectName;

    [TextArea]
    public string description;

    [Header("Visual")]
    public Sprite icon;

    /// <summary>
    /// Dipanggil sekali saat effect pertama kali diterapkan ke target.
    /// </summary>
    public abstract void OnApply(Enemy target, StatusEffectInstance instance);

    /// <summary>
    /// Dipanggil setiap akhir turn selama effect masih aktif.
    /// </summary>
    public abstract void OnTick(Enemy target, StatusEffectInstance instance);

    /// <summary>
    /// Dipanggil saat duration habis dan effect dihapus.
    /// </summary>
    public abstract void OnExpire(Enemy target, StatusEffectInstance instance);

    /// <summary>
    /// Override ini jika effect harus di-stack secara khusus
    /// (misalnya Poison: value dijumlah, Shield: value diganti).
    /// Default: buat instance baru (stack paralel).
    /// </summary>
    public virtual bool TryStack(
        StatusEffectInstance existing,
        int newDuration,
        int newValue)
    {
        return false;
    }
}