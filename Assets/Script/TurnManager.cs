using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private List<Enemy> activeEnemies = new();

    public TurnState CurrentState { get; private set; }

    // ---------------------------------------------------------------
    // LIFECYCLE
    // ---------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Bersihkan null entries dari Inspector
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
                activeEnemies.RemoveAt(i);
        }

        if (CardManager.Instance == null)
        {
            Debug.LogError("[TurnManager] CardManager tidak ditemukan!");
            return;
        }

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("[TurnManager] PlayerManager tidak ditemukan!");
            return;
        }

        // Set state tanpa trigger action apapun di Start
        // untuk menghindari initialization-order issues
        CurrentState = TurnState.PlayerTurn;

        Debug.Log("[TurnManager] Siap — giliran player dimulai");
    }

    // ---------------------------------------------------------------
    // TURN FLOW
    // ---------------------------------------------------------------

    /// <summary>
    /// Dipanggil oleh tombol "End Turn" lewat GameUIController.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (CurrentState != TurnState.PlayerTurn)
        {
            Debug.LogWarning("[TurnManager] Bukan giliran player sekarang");
            return;
        }

        CurrentState = TurnState.Busy;
        ProcessEnemyTurn();
    }

    private void ProcessEnemyTurn()
    {
        CurrentState = TurnState.EnemyTurn;

        TickStatusEffects();
        EnemyActions();
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        CurrentState = TurnState.PlayerTurn;

        // Reset AP dan Mana player
        PlayerManager.Instance?.ResetTurnResources();

        // BUG FIX: Isi tangan player di awal giliran
        CardManager.Instance?.RefillHand();

        Debug.Log("[TurnManager] Giliran player dimulai");
    }

    private void TickStatusEffects()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            activeEnemies[i].TickEffects();
        }
    }

    private void EnemyActions()
    {
        // Iterasi copy sementara karena enemy bisa mati (dan remove diri)
        // saat TickStatusEffects baru saja — activeEnemies sudah bersih,
        // tapi buat defensive copy untuk safety
        List<Enemy> snapshot = new List<Enemy>(activeEnemies);

        foreach (Enemy enemy in snapshot)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            enemy.PerformAttack();
        }
    }

    // ---------------------------------------------------------------
    // ENEMY REGISTRY
    // ---------------------------------------------------------------

    /// <summary>
    /// Tambah enemy ke daftar aktif (misalnya saat enemy di-spawn runtime).
    /// </summary>
    public void AddEnemy(Enemy enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            Debug.Log($"[TurnManager] Enemy ditambah: {enemy.name}");
        }
    }

    /// <summary>
    /// Hapus enemy dari daftar aktif.
    /// Dipanggil oleh Enemy.Die() agar musuh mati tidak terus di-tick.
    /// </summary>
    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
            Debug.Log($"[TurnManager] Enemy dihapus: {enemy.name}");
        }
    }
}