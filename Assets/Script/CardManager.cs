using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Pool & Deck Settings")]
    [SerializeField] private CardDisplay cardPrefab;
    [SerializeField] private Transform handLayoutGroup;
    [SerializeField] private List<CardData> deckList = new List<CardData>(); 
    private readonly List<CardData> masterDeck = new();
    
    [SerializeField] private int maxHandSize = 6;
    
    private IObjectPool<CardDisplay> cardPool;
    private readonly List<CardData> drawPile = new();
    private readonly List<CardData> discardPile = new();
    private readonly List<CardDisplay> handCards = new();

    private void Awake()
    {
        Debug.Log("[CardManager] Awake START");

        if (Instance != null && Instance != this)
        {
            Debug.Log("[CardManager] Instance exist, destroying duplicate");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Validate references
        if (cardPrefab == null)
        {
            Debug.LogError("[CardManager] cardPrefab is NULL! Assign in inspector!");
            return;
        }

        if (handLayoutGroup == null)
        {
            Debug.LogError("[CardManager] handLayoutGroup is NULL! Assign in inspector!");
            return;
        }

        Debug.Log("[CardManager] Creating object pool");

        cardPool = new ObjectPool<CardDisplay>(
            CreateNewCardInstance,
            OnTakeCardFromPool,
            OnReturnCardToPool,
            OnDestroyPoolObject,
            true,
            10,
            50
        );

        Debug.Log("[CardManager] Awake COMPLETE");
    }

    private void Start()
    {
        Debug.Log("[CardManager] Start BEGIN");

        // Validate deck
        if (deckList == null || deckList.Count == 0)
        {
            Debug.LogError("[CardManager] deckList is empty! Add CardData assets in inspector!");
            return;
        }

        Debug.Log($"[CardManager] deckList has {deckList.Count} cards");

        BuildDeck();
        DrawStartingHand();

        Debug.Log("[CardManager] Start COMPLETE");
    }

    public bool UseCardOnTarget(CardDisplay card, Enemy targetEnemy)
    {
        if (card == null)
        {
            Debug.LogError("[CardManager] Card is null!");
            return false;
        }

        CardData cardData = card.CurrentCardData;
        if (cardData == null)
        {
            Debug.LogError("[CardManager] CardData missing!");
            return false;
        }

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("[CardManager] PlayerManager not found!");
            return false;
        }

        if (!PlayerManager.Instance.CanAffordAndSpend(cardData.actionPointCost, cardData.manaCost))
        {
            Debug.LogWarning($"[CardManager] Not enough resources for {cardData.cardName}");
            return false;
        }

        if (cardData.effects == null || cardData.effects.Count == 0)
        {
            Debug.LogWarning($"[CardManager] Card {cardData.cardName} has no effects!");
            return false;
        }

        foreach (CardEffect effect in cardData.effects)
        {
            if (effect == null)
                continue;

            effect.ExecuteEffect(targetEnemy, card);
        }

        DiscardCard(card);
        DrawNextCard();

        return true;
    }

    public void DrawNextCard()
    {
        // Check 1: Hand full?
        if (handCards.Count >= maxHandSize)
            return;
        
        // Check 2: DrawPile kosong? Reshuffle
        if (drawPile.Count == 0)
            ReshuffleDiscardPile();
        
        // Check 3: Masih kosong? STOP
        if (drawPile.Count == 0)
        {
            Debug.LogWarning("[CardManager] No cards available!");
            return;
        }
        
        // Draw card
        CardData card = drawPile[0];
        drawPile.RemoveAt(0);
        SpawnCardToHand(card);
    }

    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0)
        {
            Debug.LogWarning("[CardManager] Discard pile empty, cannot reshuffle!");
            return;
        }

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();

        Debug.Log("[CardManager] Discard pile reshuffled");
    }

    public void SpawnCardToHand(CardData dataToSpawn)
    {
        if (dataToSpawn == null)
        {
            Debug.LogError("[CardManager] CardData to spawn is null!");
            return;
        }

        if (cardPool == null)
        {
            Debug.LogError("[CardManager] Card pool not initialized!");
            return;
        }

        CardDisplay spawnedCard = cardPool.Get();

        if (spawnedCard == null)
        {
            Debug.LogError("[CardManager] Failed to get card from pool!");
            return;
        }

        spawnedCard.transform.SetParent(handLayoutGroup, false);
        spawnedCard.Initialize(dataToSpawn);

        handCards.Add(spawnedCard);

        if (spawnedCard.TryGetComponent(out CardDraggable drag))
        {
            drag.UpdateCanvasSorting();
        }
    }

    public void RefillHand()
    {
        while (handCards.Count < maxHandSize)
        {
            DrawNextCard();
        }
    }

    private void BuildDeck()
    {
        Debug.Log("[CardManager] BuildDeck START");

        if (deckList == null || deckList.Count == 0)
        {
            Debug.LogError("[CardManager] Cannot build deck - deckList empty!");
            return;
        }

        drawPile.Clear();
        discardPile.Clear();
        handCards.Clear();

        masterDeck.Clear();
        masterDeck.AddRange(deckList);

        drawPile.AddRange(masterDeck);

        ShuffleDrawPile();

        Debug.Log($"[CardManager] BuildDeck COMPLETE - {drawPile.Count} cards");
    }

    private void ShuffleDrawPile()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            CardData temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }

    private void DrawStartingHand()
    {
        Debug.Log($"[CardManager] DrawStartingHand START - drawing {maxHandSize} cards");

        for (int i = 0; i < maxHandSize; i++)
        {
            DrawNextCard();
        }

        Debug.Log($"[CardManager] DrawStartingHand COMPLETE - {handCards.Count} cards in hand");
    }

    private void DiscardCard(CardDisplay card)
    {
        if (card == null)
            return;

        CardData data = card.CurrentCardData;
        if (data != null)
        {
            discardPile.Add(data);
        }

        handCards.Remove(card);
        cardPool.Release(card);
    }

    [ContextMenu("Deck Debug")]
    private void DebugDeck()
    {
        Debug.Log(
            $"[CardManager Debug] " +
            $"Draw: {drawPile.Count} | " +
            $"Discard: {discardPile.Count} | " +
            $"Hand: {handCards.Count}");
    }

    private CardDisplay CreateNewCardInstance()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("[CardManager] Cannot create card instance - cardPrefab is null!");
            return null;
        }

        if (handLayoutGroup == null)
        {
            Debug.LogError("[CardManager] Cannot create card instance - handLayoutGroup is null!");
            return null;
        }

        return Instantiate(cardPrefab, handLayoutGroup);
    }

    private void OnTakeCardFromPool(CardDisplay card)
    {
        if (card != null)
            card.gameObject.SetActive(true);
    }

    private void OnReturnCardToPool(CardDisplay card)
    {
        if (card != null)
            card.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(CardDisplay card)
    {
        if (card != null)
            Destroy(card.gameObject);
    }

    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public int HandCount => handCards.Count;
}