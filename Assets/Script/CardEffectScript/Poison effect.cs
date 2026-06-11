using UnityEngine;

/// <summary>
/// Effect kartu: apply Poison status ke target enemy.
/// </summary>
[CreateAssetMenu(
    fileName = "NewPoisonCardEffect",
    menuName  = "CardGame/Effects/Poison")]
public class PoisonEffect : CardEffect
{
    [SerializeField] private StatusEffect statusEffect;
    [SerializeField] private int poisonDuration = 3;
    [SerializeField] private int damagePerTurn  = 2;

    public override void ExecuteEffect(Enemy target, CardDisplay cardSource)
    {
        // BUG FIX: null check sebelum akses target
        if (target == null)
        {
            Debug.LogWarning("[PoisonEffect] Target null, effect tidak dijalankan");
            return;
        }

        if (statusEffect == null)
        {
            Debug.LogError("[PoisonEffect] StatusEffect asset belum di-assign di Inspector!");
            return;
        }

        target.AddStatusEffect(statusEffect, poisonDuration, damagePerTurn);
    }
}