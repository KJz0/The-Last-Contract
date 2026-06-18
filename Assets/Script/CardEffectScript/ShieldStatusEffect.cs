using UnityEngine;
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