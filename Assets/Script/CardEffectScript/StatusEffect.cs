using UnityEngine;
public abstract class StatusEffect : ScriptableObject
{
    [Header("Info")]
    public string effectName;

    [TextArea]
    public string description;

    [Header("Visual")]
    public Sprite icon;
    public abstract void OnApply(Enemy target, StatusEffectInstance instance);
    public abstract void OnTick(Enemy target, StatusEffectInstance instance);
    public abstract void OnExpire(Enemy target, StatusEffectInstance instance);
    public virtual bool TryStack(
        StatusEffectInstance existing,
        int newDuration,
        int newValue)
    {
        return false;
    }
}