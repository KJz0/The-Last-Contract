using UnityEngine;

/// <summary>
/// Burn: deals damage per turn, sama seperti Poison tapi
/// dianggap sebagai elemen Fire — bisa dibedakan untuk resistensi/interaksi.
/// Stack behavior: refresh duration, ambil value tertinggi.
/// </summary>
[CreateAssetMenu(
    fileName = "BurnStatus",
    menuName  = "CardGame/Status Effects/Burn")]
public class BurnStatusEffect : StatusEffect
{
    public override void OnApply(Enemy target, StatusEffectInstance instance)
    {
        Debug.Log($"[Status] {target.name} terbakar! {instance.value} damage/turn selama {instance.duration} turn");
    }

    public override void OnTick(Enemy target, StatusEffectInstance instance)
    {
        target.TakeDamage(instance.value);
        Debug.Log($"[Status] {target.name} menerima {instance.value} burn damage");
    }

    public override void OnExpire(Enemy target, StatusEffectInstance instance)
    {
        Debug.Log($"[Status] {target.name}: api padam");
    }

    /// <summary>
    /// Burn stack: refresh duration, ambil value tertinggi (tidak dijumlah).
    /// </summary>
    public override bool TryStack(
        StatusEffectInstance existing,
        int newDuration,
        int newValue)
    {
        existing.value    = Mathf.Max(existing.value, newValue);
        existing.duration = Mathf.Max(existing.duration, newDuration);
        return true;
    }
}