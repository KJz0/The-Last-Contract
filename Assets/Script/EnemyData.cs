using UnityEngine;
[CreateAssetMenu(
    fileName = "NewEnemyData",
    menuName  = "CardGame/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;

    [TextArea]
    public string description;

    [Header("Base Stats")]
    public int maxHealth    = 50;
    public int attackDamage = 5;

    [Header("Visual")]
    public Sprite portrait;

    [Header("Behavior (opsional)")]
    [Tooltip("Status effect yang langsung diapply ke diri sendiri saat spawn")]
    public StatusEffect[] passiveEffectsOnSpawn;
}