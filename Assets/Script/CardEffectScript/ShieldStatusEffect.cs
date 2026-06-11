using UnityEngine;

/// <summary>
/// Shield: menyerap damage masuk sejumlah value.
/// Berbeda dari yang lain: tidak tick per turn,
/// tapi dikurangi saat enemy kena damage (lihat Enemy.TakeDamage).
/// OnTick tidak melakukan apa-apa — Shield hanya expire saat duration habis
/// atau saat value habis diserap.
/// Stack behavior: jumlahkan shield value.
/// </summary>
[CreateAssetMenu(
    fileName = "ShieldStatus",
    menuName  = "CardGame/Status Effects/Shield")]
public class ShieldStatusEffect : StatusEffect
{
    public override void OnApply(Enemy target, StatusEffectInstance instance)
    {
        Debug.Log($"[Status] {target.name} mendapat Shield {instance.value}");
    }

    public override void OnTick(Enemy target, StatusEffectInstance instance)
    {
        // Shield tidak melakukan damage/heal per turn.
        // Pengurangan shield dihandle di Enemy.TakeDamage.
    }

    public override void OnExpire(Enemy target, StatusEffectInstance instance)
    {
        Debug.Log($"[Status] {target.name}: Shield habis");
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