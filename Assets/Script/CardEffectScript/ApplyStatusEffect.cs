using UnityEngine;

/// <summary>
/// Generic card effect untuk apply status effect apapun ke target.
/// Ini menggantikan PoisonEffect yang hardcoded — cukup satu asset ini
/// untuk semua jenis status (Poison, Burn, Heal, Shield) dengan
/// mengubah field statusEffect, duration, dan value di Inspector.
///
/// Cara pakai:
/// 1. Buat asset baru: Create > CardGame > Effects > Apply Status Effect
/// 2. Assign StatusEffect asset yang diinginkan (PoisonStatus, BurnStatus, dll)
/// 3. Atur duration dan value
/// 4. Masukkan ke list effects di CardData
/// </summary>
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