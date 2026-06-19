using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Alasan kartu ditolak saat dicoba ditempatkan ke slot fusion.
/// </summary>
public enum FusionRejectReason
{
    IsFusionResult,   // kartu adalah hasil fusion, tidak bisa difusion lagi
    SlotsFull,        // kedua slot sudah terisi
    AlreadyInSlot     // kartu yang sama sudah ada di salah satu slot
}

/// <summary>
/// Mengelola seluruh proses fusion kartu:
/// 1. Player drag 2 kartu dari hand ke slot fusion
/// 2. Saat kedua slot terisi, tombol Fusion aktif
/// 3. Klik Fusion → roll rarity (30% gagal, 50% Basic, 15% Elite, 5% Epic)
/// 4. Jika berhasil, cari FusionRecipe yang cocok dengan 2 kartu bahan,
///    ambil pool sesuai rarity hasil roll, pilih random satu kartu dari pool
/// 5. Kartu hasil otomatis masuk ke draw pile lewat CardManager
/// 6. 2 kartu bahan selalu hilang permanen (baik gagal maupun berhasil)
///
/// Kartu yang merupakan HASIL fusion (CardData.CannotBeFused == true)
/// tidak bisa dipakai sebagai bahan fusion lagi — dicegah di PlaceCardInSlot.
/// </summary>
public class FusionManager : MonoBehaviour
{
    public static FusionManager Instance { get; private set; }

    [Header("Recipes")]
    [Tooltip("Semua kombinasi fusion yang valid di game ini")]
    [SerializeField] private List<FusionRecipe> recipes = new();

    [Header("Rarity Roll Chances (total harus 100)")]
    [SerializeField] private float failChance  = 30f;
    [SerializeField] private float basicChance = 50f;
    [SerializeField] private float eliteChance = 15f;
    [SerializeField] private float epicChance  = 5f;
    private CardDisplay slotACard;
    private CardDisplay slotBCard;
    public System.Action<CardDisplay, CardDisplay>      OnSlotsChanged;
    public System.Action<CardRarity, CardData>          OnFusionResult;
    public System.Action<CardDisplay, FusionRejectReason> OnFusionRejected;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        ValidateChances();
    }

    private void ValidateChances()
    {
        float total = failChance + basicChance + eliteChance + epicChance;
        if (Mathf.Abs(total - 100f) > 0.01f)
        {
            Debug.LogWarning($"[FusionManager] Total chance = {total}%, seharusnya 100%. " +
                              $"Fail:{failChance} Basic:{basicChance} Elite:{eliteChance} Epic:{epicChance}");
        }
    }

    // ---------------------------------------------------------------
    // SLOT MANAGEMENT
    // ---------------------------------------------------------------
    public bool PlaceCardInSlot(CardDisplay card)
    {
        if (card == null) return false;

        CardData data = card.CurrentCardData;
        if (data != null && data.CannotBeFused)
        {
            Debug.LogWarning($"[FusionManager] {data.cardName} adalah hasil fusion, tidak bisa difusion lagi");
            OnFusionRejected?.Invoke(card, FusionRejectReason.IsFusionResult);
            return false;
        }

        if (slotACard == card || slotBCard == card)
        {
            Debug.LogWarning("[FusionManager] Kartu ini sudah ada di slot fusion");
            OnFusionRejected?.Invoke(card, FusionRejectReason.AlreadyInSlot);
            return false;
        }

        if (slotACard == null)
        {
            slotACard = card;
        }
        else if (slotBCard == null)
        {
            slotBCard = card;
        }
        else
        {
            Debug.LogWarning("[FusionManager] Kedua slot fusion sudah penuh");
            OnFusionRejected?.Invoke(card, FusionRejectReason.SlotsFull);
            return false;
        }

        OnSlotsChanged?.Invoke(slotACard, slotBCard);
        return true;
    }
    public void RemoveCardFromSlot(CardDisplay card)
    {
        if (slotACard == card) slotACard = null;
        if (slotBCard == card) slotBCard = null;

        OnSlotsChanged?.Invoke(slotACard, slotBCard);
    }

    public bool AreSlotsFull => slotACard != null && slotBCard != null;
    // ---------------------------------------------------------------
    // FUSION EXECUTION
    // ---------------------------------------------------------------
    public void ExecuteFusion()
    {
        if (!AreSlotsFull)
        {
            Debug.LogWarning("[FusionManager] Slot belum penuh, fusion dibatalkan");
            return;
        }

        CardData dataA = slotACard.CurrentCardData;
        CardData dataB = slotBCard.CurrentCardData;

        Debug.Log($"[FusionManager] Fusion dimulai: {dataA?.cardName} + {dataB?.cardName}");
        CardRarity resultRarity = RollRarity();
        Debug.Log($"[FusionManager] Roll hasil: {resultRarity}");

        CardData resultCard = null;

        if (resultRarity != CardRarity.None)
        {
            FusionRecipe matchedRecipe = FindMatchingRecipe(dataA, dataB);

            if (matchedRecipe == null)
            {
                Debug.LogWarning($"[FusionManager] Tidak ada recipe untuk kombinasi {dataA?.cardName} + {dataB?.cardName}");
            }
            else
            {
                resultCard = PickRandomFromPool(matchedRecipe.GetPoolForRarity(resultRarity));
            }
        }
        ConsumeSlotCards();

        if (resultCard != null)
        {
            Debug.Log($"[FusionManager] Berhasil! Kartu baru: {resultCard.cardName} ({resultRarity})");
            CardManager.Instance?.AddCardToDrawPileTop(resultCard);
        }
        else
        {
            Debug.Log("[FusionManager] Fusion gagal, tidak ada kartu yang dihasilkan");
        }

        OnFusionResult?.Invoke(resultRarity, resultCard);
        OnSlotsChanged?.Invoke(null, null);
    }
    private CardRarity RollRarity()
    {
        float roll = Random.Range(0f, 100f);

        if (roll < failChance)
            return CardRarity.None;

        roll -= failChance;
        if (roll < basicChance)
            return CardRarity.Basic;

        roll -= basicChance;
        if (roll < eliteChance)
            return CardRarity.Elite;

        return CardRarity.Epic;
    }

    private FusionRecipe FindMatchingRecipe(CardData a, CardData b)
    {
        foreach (FusionRecipe recipe in recipes)
        {
            if (recipe != null && recipe.Matches(a, b))
                return recipe;
        }
        return null;
    }

    private CardData PickRandomFromPool(List<CardData> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }
    private void ConsumeSlotCards()
    {
        CardManager.Instance?.ConsumeCardPermanently(slotACard);
        CardManager.Instance?.ConsumeCardPermanently(slotBCard);

        slotACard = null;
        slotBCard = null;
    }

    public CardDisplay SlotA => slotACard;
    public CardDisplay SlotB => slotBCard;
}