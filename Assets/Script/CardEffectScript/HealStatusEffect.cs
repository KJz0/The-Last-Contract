using UnityEngine;

/// <summary>
/// Heal: memulihkan HP per turn.
/// Berguna untuk buff musuh (boss regen) atau kartu support player.
/// Stack behavior: jumlahkan value, perpanjang durasi.
/// </summary>
[CreateAssetMenu(
    fileName = "HealStatus",
    menuName  = "CardGame/Status Effects/Heal")]
public class HealStatusEffect : StatusEffect
{
    public override void OnApply(Enemy target, StatusEffectInstance instance)
    {
        Debug.Log($"[Status] {target.name} mendapat regenerasi {instance.value} HP/turn");
    }

    public override void OnTick(Enemy target, StatusEffectInstance instance)
    {
        target.Heal(instance.value);
        Debug.Log($"[Status] {target.name} regenerasi {instance.value} HP");
    }

    public override void OnExpire(Enemy target, StatusEffectInstance instance)
    {
        Debug.Log($"[Status] {target.name}: regenerasi berakhir");
    }

    public override bool TryStack(
        StatusEffectInstance existing,
        int newDuration,
        int newValue)
    {
        existing.value    += newValue;
        existing.duration  = Mathf.Max(existing.duration, newDuration);
        return true;
    }
}