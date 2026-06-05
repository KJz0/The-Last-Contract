using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPoisonCardEffect",
    menuName = "CardGame/Effects/Poison")]
public class PoisonEffect : CardEffect
{
    [SerializeField]
    private StatusEffect statusEffect;

    [SerializeField]
    private int poisonDuration = 3;

    [SerializeField]
    private int damagePerTurn = 2;

    public override void ExecuteEffect(
        Enemy target,
        CardDisplay card)
    {
        target.AddStatusEffect(
            statusEffect,
            poisonDuration,
            damagePerTurn);
    }
}