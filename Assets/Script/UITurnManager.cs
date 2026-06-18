using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private Button   endTurnButton;
    [SerializeField] private TMP_Text turnStateText;

    private void Awake()
    {
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
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
            Debug.LogError("[GameUIController] TurnManager tidak ditemukan!");
            return;
        }

        if (TurnManager.Instance.CurrentState == TurnState.PlayerTurn)
        {
            TurnManager.Instance.EndPlayerTurn();
        }
    }

    private void UpdateTurnStateDisplay()
    {
        if (turnStateText == null || TurnManager.Instance == null) return;

        turnStateText.text = TurnManager.Instance.CurrentState switch
        {
            TurnState.PlayerTurn => "Your Turn",
            TurnState.EnemyTurn  => "Enemy Turn",
            TurnState.Busy       => "Processing...",
            _                    => "Unknown"
        };
    }

    private void UpdateEndTurnButtonState()
    {
        if (endTurnButton == null || TurnManager.Instance == null) return;

        endTurnButton.interactable =
            TurnManager.Instance.CurrentState == TurnState.PlayerTurn;
    }
}