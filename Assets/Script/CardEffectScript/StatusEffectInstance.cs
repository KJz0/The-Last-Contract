using System;
[Serializable]
public class StatusEffectInstance
{
    public StatusEffect effect;
    public int duration;

    public int value;

    public StatusEffectInstance(StatusEffect effect, int duration, int value)
    {
        this.effect   = effect;
        this.duration = duration;
        this.value    = value;
    }
}