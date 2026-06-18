using UnityEngine;
[CreateAssetMenu(
    fileName = "DamageEffect",
    menuName  = "CardGame/Effects/Damage")]
public class DamageEffect : CardEffect
{
    [SerializeField] private int damage;

    public override void ExecuteEffect(Enemy target, CardDisplay cardSource)
    {
        if (target == null)
        {
            Debug.LogWarning("[DamageEffect] Target null, effect tidak dijalankan");
            return;
        }

        target.TakeDamage(damage);
    }
}