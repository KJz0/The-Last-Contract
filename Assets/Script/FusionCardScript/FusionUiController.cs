using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FusionUIController : MonoBehaviour
{
    [Header("Panel Toggle")]
    [SerializeField] private GameObject fusionPanel;
    [SerializeField] private Button     openFusionButton;
    [SerializeField] private Button     closeFusionButton;

    [Header("Fusion Action")]
    [SerializeField] private Button confirmFusionButton;

    [Header("Slot Visual Feedback (opsional)")]
    [SerializeField] private TMP_Text slotAStatusText;
    [SerializeField] private TMP_Text slotBStatusText;

    [Header("Result Popup")]
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private TMP_Text   resultText;
    [SerializeField] private float      resultPopupDuration = 2f;

    private void Awake()
    {
        if (openFusionButton != null)
            openFusionButton.onClick.AddListener(OpenFusionPanel);

        if (closeFusionButton != null)
            closeFusionButton.onClick.AddListener(CloseFusionPanel);

        if (confirmFusionButton != null)
            confirmFusionButton.onClick.AddListener(OnConfirmFusionClicked);

        if (fusionPanel != null)
            fusionPanel.SetActive(false);

        if (resultPopup != null)
            resultPopup.SetActive(false);
    }

    private void OnEnable()
    {
        if (FusionManager.Instance != null)
        {
            FusionManager.Instance.OnSlotsChanged   += HandleSlotsChanged;
            FusionManager.Instance.OnFusionResult   += HandleFusionResult;
            FusionManager.Instance.OnFusionRejected += HandleFusionRejected;
        }
    }

    private void OnDisable()
    {
        if (FusionManager.Instance != null)
        {
            FusionManager.Instance.OnSlotsChanged   -= HandleSlotsChanged;
            FusionManager.Instance.OnFusionResult   -= HandleFusionResult;
            FusionManager.Instance.OnFusionRejected -= HandleFusionRejected;
        }
    }

    private void Start()
    {
        if (FusionManager.Instance != null)
        {
            FusionManager.Instance.OnSlotsChanged   += HandleSlotsChanged;
            FusionManager.Instance.OnFusionResult   += HandleFusionResult;
            FusionManager.Instance.OnFusionRejected += HandleFusionRejected;
        }

        UpdateConfirmButtonState();
    }
    // ---------------------------------------------------------------
    // PANEL TOGGLE
    // ---------------------------------------------------------------
    private void OpenFusionPanel()
    {
        if (fusionPanel != null)
            fusionPanel.SetActive(true);
    }
    private void CloseFusionPanel()
    {
        if (fusionPanel != null)
            fusionPanel.SetActive(false);
    }
    // ---------------------------------------------------------------
    // FUSION ACTION
    // ---------------------------------------------------------------
    private void OnConfirmFusionClicked()
    {
        FusionManager.Instance?.ExecuteFusion();
    }
    // ---------------------------------------------------------------
    // EVENT HANDLERS
    // ---------------------------------------------------------------
    private void HandleSlotsChanged(CardDisplay slotA, CardDisplay slotB)
    {
        if (slotAStatusText != null)
            slotAStatusText.text = slotA != null ? slotA.CurrentCardData.cardName : "Kosong";

        if (slotBStatusText != null)
            slotBStatusText.text = slotB != null ? slotB.CurrentCardData.cardName : "Kosong";

        UpdateConfirmButtonState();
    }

    private void HandleFusionResult(CardRarity rarity, CardData resultCard)
    {
        ShowResultPopup(rarity, resultCard);
        UpdateConfirmButtonState();
    }
    private void HandleFusionRejected(CardDisplay card, FusionRejectReason reason)
    {
        if (resultPopup == null || resultText == null) return;

        string cardName = card?.CurrentCardData?.cardName ?? "Kartu";

        resultText.text = reason switch
        {
            FusionRejectReason.IsFusionResult => $"{cardName} adalah hasil fusion, tidak bisa difusion lagi",
            FusionRejectReason.SlotsFull       => "Slot fusion sudah penuh",
            FusionRejectReason.AlreadyInSlot   => "Kartu ini sudah ada di slot fusion",
            _                                  => "Tidak bisa menempatkan kartu"
        };

        resultPopup.SetActive(true);
        CancelInvoke(nameof(HideResultPopup));
        Invoke(nameof(HideResultPopup), resultPopupDuration);
    }

    // ---------------------------------------------------------------
    // PRIVATE
    // ---------------------------------------------------------------

    private void UpdateConfirmButtonState()
    {
        if (confirmFusionButton == null || FusionManager.Instance == null) return;
        confirmFusionButton.interactable = FusionManager.Instance.AreSlotsFull;
    }

    private void ShowResultPopup(CardRarity rarity, CardData resultCard)
    {
        if (resultPopup == null || resultText == null) return;

        resultText.text = rarity == CardRarity.None
            ? "Fusion gagal..."
            : $"Berhasil! Kartu {rarity}: {resultCard?.cardName}";

        resultPopup.SetActive(true);
        CancelInvoke(nameof(HideResultPopup));
        Invoke(nameof(HideResultPopup), resultPopupDuration);
    }

    private void HideResultPopup()
    {
        if (resultPopup != null)
            resultPopup.SetActive(false);
    }
}