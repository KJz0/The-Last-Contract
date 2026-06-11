using UnityEngine;

/// <summary>
/// ScriptableObject config untuk satu jenis musuh.
/// Buat asset baru untuk setiap jenis musuh (Goblin, Skeleton, Boss, dll).
/// Enemy MonoBehaviour akan membaca data dari sini di Awake/Start.
/// </summary>
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