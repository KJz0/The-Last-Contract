using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TMP_Text turnStateText;

    private void Awake()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }
    }

    private void Update()
    {
        UpdateTurnStateDisplay();
        UpdateEndTurnButtonState();
    }

    private void OnEndTurnButtonClicked()
    {
        if (TurnManager.Instance == null)
        {
            Debug.LogError("[GameUIController] TurnManager not found!");
            return;
        }

        if (TurnManager.Instance.CurrentState == TurnState.PlayerTurn)
        {
            TurnManager.Instance.EndPlayerTurn();
        }
        else
        {
            Debug.LogWarning("[GameUIController] Cannot end turn - not player turn!");
        }
    }

    private void UpdateTurnStateDisplay()
    {
        if (turnStateText == null || TurnManager.Instance == null)
            return;

        TurnState currentState = TurnManager.Instance.CurrentState;
        turnStateText.text = currentState switch
        {
            TurnState.PlayerTurn => "Your Turn",
            TurnState.EnemyTurn => "Enemy Turn",
            TurnState.Busy => "Processing...",
            _ => "Unknown"
        };
    }

    private void UpdateEndTurnButtonState()
    {
        if (endTurnButton == null || TurnManager.Instance == null)
            return;

        bool isPlayerTurn = TurnManager.Instance.CurrentState == TurnState.PlayerTurn;
        endTurnButton.interactable = isPlayerTurn;
    }
}