using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private List<Enemy> activeEnemies = new();

    public TurnState CurrentState { get; private set; }

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
        StartPlayerTurn();
    }

    public void EndPlayerTurn()
    {
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
        CurrentState = TurnState.PlayerTurn;

        PlayerManager.Instance.ResetTurnResources();
        CardManager.Instance.RefillHand();

        Debug.Log("[TurnManager] Player turn started");
    }

    private void ProcessEnemyTurn()
    {
        CurrentState = TurnState.EnemyTurn;

        TickStatusEffects();
        EnemyActions();

        StartPlayerTurn();
    }

    private void TickStatusEffects()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null)
                continue;

            enemy.TickEffects();
        }

        Debug.Log("[TurnManager] Status effects ticked");
    }

    private void EnemyActions()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null)
                continue;

            enemy.PerformAttack();
        }

        Debug.Log("[TurnManager] Enemy actions completed");
    }

    public void AddEnemy(Enemy enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
        }
    }
}