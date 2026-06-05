using UnityEngine;

[CreateAssetMenu(fileName = "NewPoisonEffect", menuName = "CardGame/Effects/Poison")]
public class PoisonEffect : CardEffect
{
    [Header("Poison Mechanics")]
    public int poisonDuration = 3;
    public int damagePerTurn = 2;

    public override void ExecuteEffect(Enemy target, CardDisplay card)
    {
        Debug.Log($"[Effect Engine] {card.CurrentCardData.cardName} sukses ngasih racun {poisonDuration} turn ke {target.gameObject.name}!");
        target.ApplyStatusEffect("Poison", poisonDuration);
    }
}