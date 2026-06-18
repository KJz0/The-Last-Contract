using UnityEngine;


[CreateAssetMenu(
    fileName = "NewApplyStatusEffect",
    menuName  = "CardGame/Effects/Apply Status Effect")]
public class ApplyStatusEffect : CardEffect
{
    [SerializeField] private StatusEffect statusEffect;
    [SerializeField] private int          duration = 3;
    [SerializeField] private int          value    = 2;

    public override void ExecuteEffect(Enemy target, CardDisplay cardSource)
    {
        if (target == null)
        {
            Debug.LogWarning("[ApplyStatusEffect] Target null");
            return;
        }

        if (statusEffect == null)
        {
            Debug.LogError("[ApplyStatusEffect] StatusEffect belum di-assign!");
            return;
        }

        target.AddStatusEffect(statusEffect, duration, value);
    }
}