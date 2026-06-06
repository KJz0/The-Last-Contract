using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Player Vitals")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxAP = 8;
    [SerializeField] private int maxMana = 8;
    
    private int currentHealth;
    private int currentAP;
    private int currentMana;

    [Header("Health Sliders")]
    [SerializeField] private Slider mainHpSlider;
    [SerializeField] private Slider ghostHpSlider;

    [Header("Resource Sliders")]
    [SerializeField] private Slider apSlider;   
    [SerializeField] private Slider manaSlider;

    [Header("Resource Text")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text apText;   
    [SerializeField] private TMP_Text manaText;

    [Header("Juicy Tuning")]
    [SerializeField] private float ghostLerpSpeed = 3f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        InitializeResources();
        SetupSliders();
        UpdateUIFields();
    }

    private void Update()
    {
        UpdateGhostHealthBar();
    }

    // --- INITIALIZATION ---

    private void InitializeResources()
    {
        currentHealth = maxHealth;
        currentAP = maxAP;
        currentMana = maxMana;
    }

    private void SetupSliders()
    {
        if (mainHpSlider != null) 
        { 
            mainHpSlider.maxValue = maxHealth; 
            mainHpSlider.value = currentHealth; 
        }

        if (ghostHpSlider != null) 
        { 
            ghostHpSlider.maxValue = maxHealth; 
            ghostHpSlider.value = currentHealth; 
        }

        if (apSlider != null) 
        { 
            apSlider.maxValue = maxAP; 
            apSlider.value = currentAP; 
        }

        if (manaSlider != null) 
        { 
            manaSlider.maxValue = maxMana; 
            manaSlider.value = currentMana; 
        }
    }

    // --- HEALTH MANAGEMENT ---

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        if (mainHpSlider != null) 
        {
            mainHpSlider.value = currentHealth;
        }

        UpdateUIFields();
    }

    private void UpdateGhostHealthBar()
    {
        if (ghostHpSlider == null || mainHpSlider == null) return;

        if (ghostHpSlider.value != mainHpSlider.value)
        {
            ghostHpSlider.value = Mathf.Lerp(ghostHpSlider.value, mainHpSlider.value, Time.deltaTime * ghostLerpSpeed);
        }
    }

    // --- RESOURCE MANAGEMENT ---

    public bool CanAffordAndSpend(int apCost, int manaCost)
    {
        if (!CanAfford(apCost, manaCost))
        {
            return false;
        }

        SpendResources(apCost, manaCost);
        return true;
    }

    private bool CanAfford(int apCost, int manaCost)
    {
        return currentAP >= apCost && currentMana >= manaCost;
    }

    private void SpendResources(int apCost, int manaCost)
    {
        currentAP -= apCost;
        currentMana -= manaCost;

        if (apSlider != null) apSlider.value = currentAP;
        if (manaSlider != null) manaSlider.value = currentMana;

        UpdateUIFields();
    }

    public void ResetTurnResources()
    {
        currentAP = maxAP;
        currentMana = maxMana;

        if (apSlider != null) apSlider.value = currentAP;
        if (manaSlider != null) manaSlider.value = currentMana;

        UpdateUIFields();
    }

    // --- UI UPDATES ---

    private void UpdateUIFields()
    {
        if (hpText != null) hpText.text = $"{currentHealth} / {maxHealth}";
        if (apText != null) apText.text = $"AP: {currentAP} / {maxAP}";
        if (manaText != null) manaText.text = $"MANA: {currentMana} / {maxMana}";
    }
}