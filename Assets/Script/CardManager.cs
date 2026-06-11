using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Mengelola deck, draw pile, discard pile, dan tangan pemain.
/// Menggunakan Unity ObjectPool untuk kartu agar efisien.
/// 
/// BUG FIX: Dihapus panggilan draggable.UpdateCanvasSorting() yang
/// tidak ada di CardDraggable. RefillHand sekarang dipanggil oleh
/// TurnManager.StartPlayerTurn (bukan dikomentari).
/// </summary>
public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Pool & Deck Settings")]
    [SerializeField] private CardDisplay    cardPrefab;
    [SerializeField] private Transform      handLayoutGroup;
    [SerializeField] private List<CardData> deckList = new List<CardData>();

    [SerializeField] private int maxHandSize = 6;

    private readonly List<CardData>    masterDeck  = new();
    private IObjectPool<CardDisplay>   cardPool;
    private readonly List<CardData>    drawPile    = new();
    private readonly List<CardData>    discardPile = new();
    private readonly List<CardDisplay> handCards   = new();

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

        if (cardPrefab == null)
        {
            Debug.LogError("[CardManager] cardPrefab belum di-assign!");
            return;
        }

        if (handLayoutGroup == null)
        {
            Debug.LogError("[CardManager] handLayoutGroup belum di-assign!");
            return;
        }

        cardPool = new ObjectPool<CardDisplay>(
            CreateNewCardInstance,
            OnTakeCardFromPool,
            OnReturnCardToPool,
            OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50
        );
    }

    private void Start()
    {
        if (deckList == null || deckList.Count == 0)
        {
            Debug.LogError("[CardManager] deckList kosong! Isi di Inspector.");
            return;
        }

        BuildDeck();
        DrawStartingHand();
    }

    // ---------------------------------------------------------------
    // PUBLIC API
    // ---------------------------------------------------------------

    /// <summary>
    /// Pakai kartu ke target enemy. Dipanggil oleh CardDraggable saat drop.
    /// Return true jika berhasil, false jika resource tidak cukup atau invalid.
    /// </summary>
    public bool UseCardOnTarget(CardDisplay card, Enemy targetEnemy)
    {
        if (card == null)
        {
            Debug.LogError("[CardManager] Card null");
            return false;
        }

        CardData cardData = card.CurrentCardData;
        if (cardData == null)
        {
            Debug.LogError("[CardManager] CardData missing");
            return false;
        }

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("[CardManager] PlayerManager tidak ditemukan");
            return false;
        }

        if (!PlayerManager.Instance.CanAffordAndSpend(cardData.actionPointCost, cardData.manaCost))
        {
            Debug.LogWarning($"[CardManager] Resource tidak cukup untuk {cardData.cardName}");
            return false;
        }

        if (cardData.effects == null || cardData.effects.Count == 0)
        {
            Debug.LogWarning($"[CardManager] {cardData.cardName} tidak punya effects");
            return false;
        }

        foreach (CardEffect effect in cardData.effects)
        {
            if (effect == null) continue;
            effect.ExecuteEffect(targetEnemy, card);
        }

        DiscardCard(card);
        DrawNextCard();

        return true;
    }

    /// <summary>
    /// Ambil satu kartu dari draw pile ke tangan.
    /// </summary>
    public void DrawNextCard()
    {
        if (handCards.Count >= maxHandSize) return;

        if (drawPile.Count == 0)
            ReshuffleDiscardPile();

        if (drawPile.Count == 0)
        {
            Debug.LogWarning("[CardManager] Tidak ada kartu tersisa");
            return;
        }

        CardData card = drawPile[0];
        drawPile.RemoveAt(0);
        SpawnCardToHand(card);
    }

    /// <summary>
    /// Isi tangan sampai maxHandSize. Dipanggil oleh TurnManager saat giliran player dimulai.
    /// </summary>
    public void RefillHand()
    {
        while (handCards.Count < maxHandSize)
        {
            if (drawPile.Count == 0 && discardPile.Count == 0)
                break;

            DrawNextCard();
        }
    }

    public void SpawnCardToHand(CardData dataToSpawn)
    {
         Debug.Log($"[CardDisplay] Initialize dipanggil dengan: {dataToSpawn?.cardName ?? "NULL"}");
        if (dataToSpawn == null || cardPool == null) return;

        CardDisplay spawnedCard = cardPool.Get();
        if (spawnedCard == null) return;

        spawnedCard.transform.SetParent(handLayoutGroup, false);
        spawnedCard.Initialize(dataToSpawn);
        handCards.Add(spawnedCard);
    }

    // ---------------------------------------------------------------
    // DECK MANAGEMENT
    // ---------------------------------------------------------------

    private void BuildDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        handCards.Clear();
        masterDeck.Clear();

        masterDeck.AddRange(deckList);
        drawPile.AddRange(masterDeck);
        ShuffleDrawPile();

        Debug.Log($"[CardManager] Deck siap: {drawPile.Count} kartu");
    }

    private void DrawStartingHand()
    {
        for (int i = 0; i < maxHandSize; i++)
            DrawNextCard();
    }

    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0) return;

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();

        Debug.Log("[CardManager] Discard pile dikocok kembali");
    }

    private void ShuffleDrawPile()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int      j    = Random.Range(0, i + 1);
            CardData temp = drawPile[i];
            drawPile[i]   = drawPile[j];
            drawPile[j]   = temp;
        }
    }

    private void DiscardCard(CardDisplay card)
    {
        if (card == null) return;

        CardData data = card.CurrentCardData;
        if (data != null)
            discardPile.Add(data);

        handCards.Remove(card);
        cardPool.Release(card);
    }

    // ---------------------------------------------------------------
    // POOL CALLBACKS
    // ---------------------------------------------------------------

    private CardDisplay CreateNewCardInstance()
    {
        if (cardPrefab == null || handLayoutGroup == null) return null;
        return Instantiate(cardPrefab, handLayoutGroup);
    }

    private void OnTakeCardFromPool(CardDisplay card)
    {
        if (card == null) return;
        card.gameObject.SetActive(true);

        // Pastikan CanvasGroup tidak memblokir input
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha          = 1f;
            cg.blocksRaycasts = true;
            cg.interactable   = true;
        }
    }

    private void OnReturnCardToPool(CardDisplay card)
    {
        if (card != null) card.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(CardDisplay card)
    {
        if (card != null) Destroy(card.gameObject);
    }

    // ---------------------------------------------------------------
    // READ-ONLY PROPERTIES
    // ---------------------------------------------------------------

    public int DrawPileCount    => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public int HandCount        => handCards.Count;

    [ContextMenu("Deck Debug")]
    private void DebugDeck()
    {
        Debug.Log($"[CardManager] Draw: {drawPile.Count} | Discard: {discardPile.Count} | Hand: {handCards.Count}");
    }
}