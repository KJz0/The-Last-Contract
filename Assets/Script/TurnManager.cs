using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField]
    private List<Enemy> activeEnemies = new();

    public TurnState CurrentState { get; private set; }

    private void Awake()
    {
        Debug.Log("[TurnManager] Awake START");

        if (Instance != null && Instance != this)
        {
            Debug.Log("[TurnManager] Destroying duplicate instance");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[TurnManager] Awake COMPLETE");
    }

    private void Start()
    {
        Debug.Log("[TurnManager] Start BEGIN");

        // VALIDATE
        if (activeEnemies == null)
        {
            Debug.LogError("[TurnManager] activeEnemies list is NULL!");
            activeEnemies = new List<Enemy>();
        }

        Debug.Log($"[TurnManager] activeEnemies count: {activeEnemies.Count}");

        // Check for null enemies
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                Debug.LogWarning("[TurnManager] Removing null enemy from list");
                activeEnemies.RemoveAt(i);
            }
        }

        Debug.Log($"[TurnManager] After cleanup: {activeEnemies.Count} enemies");

        // Check CardManager
        if (CardManager.Instance == null)
        {
            Debug.LogError("[TurnManager] CardManager not found!");
            return;
        }

        Debug.Log("[TurnManager] CardManager found");

        // Check PlayerManager
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("[TurnManager] PlayerManager not found!");
            return;
        }

        Debug.Log("[TurnManager] PlayerManager found");

        // DON'T call StartPlayerTurn yet - just set state
        Debug.Log("[TurnManager] Setting initial state");
        CurrentState = TurnState.PlayerTurn;

        Debug.Log("[TurnManager] Start COMPLETE - READY");
    }

    public void EndPlayerTurn()
    {
        Debug.Log("[TurnManager] EndPlayerTurn called");

        if (CurrentState != TurnState.PlayerTurn)
        {
            Debug.LogWarning("[TurnManager] Cannot end turn - not player turn!");
            return;
        }

        CurrentState = TurnState.Busy;

        ProcessEnemyTurn();
    }

    private void StartPlayerTurn()
    {
        Debug.Log("[TurnManager] StartPlayerTurn BEGIN");

        CurrentState = TurnState.PlayerTurn;

        if (PlayerManager.Instance != null)
        {
            Debug.Log("[TurnManager] Resetting turn resources");
            PlayerManager.Instance.ResetTurnResources();
        }

        // DON'T call RefillHand - let player action trigger it
        // if (CardManager.Instance != null)
        // {
        //     CardManager.Instance.RefillHand();
        // }

        Debug.Log("[TurnManager] StartPlayerTurn COMPLETE");
    }

    private void ProcessEnemyTurn()
    {
        Debug.Log("[TurnManager] ProcessEnemyTurn BEGIN");

        CurrentState = TurnState.EnemyTurn;

        Debug.Log("[TurnManager] Ticking status effects");
        TickStatusEffects();

        Debug.Log("[TurnManager] Enemy actions");
        EnemyActions();

        Debug.Log("[TurnManager] Starting player turn");
        StartPlayerTurn();

        Debug.Log("[TurnManager] ProcessEnemyTurn COMPLETE");
    }

    private void TickStatusEffects()
    {
        Debug.Log($"[TurnManager] TickStatusEffects - {activeEnemies.Count} enemies");

        if (activeEnemies == null || activeEnemies.Count == 0)
        {
            Debug.Log("[TurnManager] No enemies to tick");
            return;
        }

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[TurnManager] Null enemy in list!");
                continue;
            }

            Debug.Log($"[TurnManager] Ticking effects for {enemy.name}");
            enemy.TickEffects();
        }

        Debug.Log("[TurnManager] TickStatusEffects COMPLETE");
    }

    private void EnemyActions()
    {
        Debug.Log($"[TurnManager] EnemyActions - {activeEnemies.Count} enemies");

        if (activeEnemies == null || activeEnemies.Count == 0)
        {
            Debug.Log("[TurnManager] No enemies to attack");
            return;
        }

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[TurnManager] Null enemy in list!");
                continue;
            }

            Debug.Log($"[TurnManager] {enemy.name} attacking");
            enemy.PerformAttack();
        }

        Debug.Log("[TurnManager] EnemyActions COMPLETE");
    }

    public void AddEnemy(Enemy enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            Debug.Log($"[TurnManager] Added enemy: {enemy.name}");
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
            Debug.Log($"[TurnManager] Removed enemy: {enemy.name}");
        }
    }
}