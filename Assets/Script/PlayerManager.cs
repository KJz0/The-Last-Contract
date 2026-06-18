using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Player Vitals")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxAP     = 8;
    [SerializeField] private int maxMana   = 8;

    private int currentHealth;
    private int currentAP;
    private int currentMana;

    [Header("Health Sliders")]
    [SerializeField] private Slider mainHpSlider;
    [SerializeField] private Slider ghostHpSlider;

    [Header("AP & Mana Orb Display")]
    [Tooltip("Component yang mengatur visual AP (bulat polos) dan Mana (gelombang air)")]
    [SerializeField] private ResourceOrbDisplay resourceOrbDisplay;

    [Header("Resource Text (opsional, terpisah dari orb)")]
    [SerializeField] private TMP_Text hpText;

    [Header("Animation Tuning")]
    [SerializeField] private float mainSmoothTime  = 0.1f;
    [SerializeField] private float ghostDelay      = 0.5f;
    [SerializeField] private float ghostSmoothTime = 0.3f;

    private float hpTarget        = 0f;
    private float mainVelocity    = 0f;
    private float ghostVelocity   = 0f;
    private float ghostDelayTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentAP     = maxAP;
        currentMana   = maxMana;
        hpTarget      = maxHealth;

        if (mainHpSlider  != null) { mainHpSlider.maxValue  = maxHealth; mainHpSlider.value  = maxHealth; }
        if (ghostHpSlider != null) { ghostHpSlider.maxValue = maxHealth; ghostHpSlider.value = maxHealth; }

        resourceOrbDisplay?.SetAP(currentAP, maxAP);
        resourceOrbDisplay?.SetMana(currentMana, maxMana);

        UpdateHpText();
    }

    private void Update()
    {
        AnimateHealthBars();
    }

    private void AnimateHealthBars()
    {
        if (mainHpSlider != null && !Mathf.Approximately(mainHpSlider.value, hpTarget))
        {
            mainHpSlider.value = Mathf.SmoothDamp(mainHpSlider.value, hpTarget, ref mainVelocity, mainSmoothTime);
            if (Mathf.Abs(mainHpSlider.value - hpTarget) < 0.1f)
            {
                mainHpSlider.value = hpTarget;
                mainVelocity = 0f;
            }
        }

        if (ghostHpSlider != null)
        {
            if (ghostDelayTimer > 0f)
            {
                ghostDelayTimer -= Time.deltaTime;
            }
            else if (!Mathf.Approximately(ghostHpSlider.value, hpTarget))
            {
                ghostHpSlider.value = Mathf.SmoothDamp(ghostHpSlider.value, hpTarget, ref ghostVelocity, ghostSmoothTime);
                if (Mathf.Abs(ghostHpSlider.value - hpTarget) < 0.1f)
                {
                    ghostHpSlider.value = hpTarget;
                    ghostVelocity = 0f;
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // HEALTH
    // ---------------------------------------------------------------

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHealth   = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        hpTarget        = currentHealth;
        ghostDelayTimer = ghostDelay;

        UpdateHpText();

        if (currentHealth <= 0) OnPlayerDied();
    }

    // ---------------------------------------------------------------
    // RESOURCES
    // ---------------------------------------------------------------

    public bool CanAffordAndSpend(int apCost, int manaCost)
    {
        if (currentAP < apCost || currentMana < manaCost)
        {
            Debug.LogWarning($"[PlayerManager] Resource tidak cukup — AP:{currentAP}/{apCost} Mana:{currentMana}/{manaCost}");
            return false;
        }

        currentAP   -= apCost;
        currentMana -= manaCost;

        resourceOrbDisplay?.SetAP(currentAP, maxAP);
        resourceOrbDisplay?.SetMana(currentMana, maxMana);

        return true;
    }

    public void ResetTurnResources()
    {
        currentAP   = maxAP;
        currentMana = maxMana;

        resourceOrbDisplay?.SetAP(currentAP, maxAP);
        resourceOrbDisplay?.SetMana(currentMana, maxMana);
    }

    // ---------------------------------------------------------------
    // UI
    // ---------------------------------------------------------------

    private void UpdateHpText()
    {
        if (hpText != null) hpText.text = $"{currentHealth} / {maxHealth}";
    }

    private void OnPlayerDied()
    {
        Debug.Log("[PlayerManager] Player mati! Game over.");
    }

    public int CurrentHealth => currentHealth;
    public int CurrentAP     => currentAP;
    public int CurrentMana   => currentMana;
}