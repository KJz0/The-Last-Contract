using UnityEngine;
[CreateAssetMenu(
    fileName = "PoisonStatus",
    menuName  = "CardGame/Status Effects/Poison")]
public class PoisonStatusEffect : StatusEffect
{
    public override void OnApply(Enemy target, StatusEffectInstance instance)
    {
        Debug.Log($"[Status] {target.name} terkena Poison {instance.value} selama {instance.duration} turn");
    }

    public override void OnTick(Enemy target, StatusEffectInstance instance)
    {
        target.TakeDamage(instance.value);
        Debug.Log($"[Status] {target.name} menerima {instance.value} poison damage ({instance.duration - 1} turn tersisa)");
    }

    public override void OnExpire(Enemy target, StatusEffectInstance instance)
    {
        Debug.Log($"[Status] {target.name}: Poison habis");
    }

    /// <summary>
    /// Poison stack: jumlahkan value, perpanjang durasi jika lebih lama.
    /// </summary>
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