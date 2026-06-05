using UnityEngine;

[CreateAssetMenu(
    fileName = "DamageEffect",
    menuName = "CardGame/Effects/Damage")]
public class DamageEffect : CardEffect
{
    [SerializeField]
    private int damage;

    public override void ExecuteEffect(
        Enemy target,
        CardDisplay cardSource)
    {
        if (target == null)
            return;

        target.TakeDamage(damage);
    }
}