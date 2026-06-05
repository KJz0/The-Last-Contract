using UnityEngine;

[CreateAssetMenu(
    fileName = "PoisonStatus",
    menuName = "Status Effects/Poison")]
public class PoisonStatusEffect : StatusEffect
{
    public override void OnApply(
        Enemy target,
        StatusEffectInstance instance)
    {
        Debug.Log(
            $"{target.name} terkena Poison");
    }

    public override void OnTick(
        Enemy target,
        StatusEffectInstance instance)
    {
        target.TakeDamage(instance.value);

        Debug.Log(
            $"{target.name} menerima {instance.value} poison damage");
    }

    public override void OnExpire(
        Enemy target,
        StatusEffectInstance instance)
    {
        Debug.Log(
            $"{target.name} poison berakhir");
    }
}