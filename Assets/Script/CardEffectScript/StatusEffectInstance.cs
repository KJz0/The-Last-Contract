using System;

/// <summary>
/// Runtime data untuk satu instance status effect yang sedang aktif di enemy.
/// Satu enemy bisa punya banyak instance dari effect yang berbeda,
/// atau bahkan beberapa instance dari effect yang sama (jika tidak di-stack).
/// </summary>
[Serializable]
public class StatusEffectInstance
{
    public StatusEffect effect;
    public int duration;

    /// <summary>
    /// Nilai numerik effect (damage per turn untuk Poison/Burn,
    /// jumlah HP untuk Heal, jumlah shield untuk Shield, dll).
    /// </summary>
    public int value;

    public StatusEffectInstance(StatusEffect effect, int duration, int value)
    {
        this.effect   = effect;
        this.duration = duration;
        this.value    = value;
    }
}