using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn,
    Busy
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private List<Enemy> activeEnemies = new();

    [Header("Timing")]
    [SerializeField] private float delayBeforeEnemyAttack = 1.5f;
    [SerializeField] private float delayBetweenEnemies    = 0.5f;
    [SerializeField] private float delayAfterEnemyTurn    = 1.0f;
    [Tooltip("Jeda tambahan menunggu animasi hand refresh selesai sebelum enemy mulai")]
    [SerializeField] private float handRefreshBuffer       = 0.3f;

    public TurnState CurrentState { get; private set; }

    public System.Action<TurnState> OnTurnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
            if (activeEnemies[i] == null) activeEnemies.RemoveAt(i);

        if (CardManager.Instance == null)  { Debug.LogError("[TurnManager] CardManager tidak ditemukan!"); return; }
        if (PlayerManager.Instance == null){ Debug.LogError("[TurnManager] PlayerManager tidak ditemukan!"); return; }

        SetState(TurnState.PlayerTurn);
    }

    // ---------------------------------------------------------------
    // PUBLIC API
    // ---------------------------------------------------------------

    public void EndPlayerTurn()
    {
        if (CurrentState != TurnState.PlayerTurn)
        {
            Debug.LogWarning("[TurnManager] Bukan giliran player");
            return;
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    public void AddEnemy(Enemy enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy != null)
            activeEnemies.Remove(enemy);
    }

    // ---------------------------------------------------------------
    // TURN FLOW
    // ---------------------------------------------------------------

    private IEnumerator EnemyTurnRoutine()
    {
        SetState(TurnState.Busy);

        // Kartu di tangan menghilang dan diganti kartu baru — animasi berjalan
        // di background sambil giliran beralih ke enemy
        CardManager.Instance?.RefreshHandWithAnimation();

        SetState(TurnState.EnemyTurn);

        // Tunggu animasi hand refresh + jeda sebelum enemy attack
        yield return new WaitForSeconds(delayBeforeEnemyAttack + handRefreshBuffer);

        TickStatusEffects();

        List<Enemy> snapshot = new List<Enemy>(activeEnemies);
        foreach (Enemy enemy in snapshot)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            enemy.PerformAttack();
            yield return new WaitForSeconds(delayBetweenEnemies);
        }

        yield return new WaitForSeconds(delayAfterEnemyTurn);

        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        PlayerManager.Instance?.ResetTurnResources();
        SetState(TurnState.PlayerTurn);
    }

    private void TickStatusEffects()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null) { activeEnemies.RemoveAt(i); continue; }
            activeEnemies[i].TickEffects();
        }
    }

    private void SetState(TurnState newState)
    {
        CurrentState = newState;
        OnTurnStateChanged?.Invoke(newState);
        Debug.Log($"[TurnManager] State: {newState}");
    }
}