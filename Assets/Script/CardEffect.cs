using UnityEngine;

/// <summary>
/// Abstract base untuk semua efek kartu.
/// Buat ScriptableObject baru yang inherit ini untuk setiap jenis efek
/// (DamageEffect, PoisonEffect, HealEffect, DrawCardEffect, dll).
/// </summary>
public abstract class CardEffect : ScriptableObject
{
    public abstract void ExecuteEffect(Enemy target, CardDisplay cardSource);
}