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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cardPool = new ObjectPool<CardDisplay>(
            CreateNewCardInstance,
            OnTakeCardFromPool,
            OnReturnCardToPool,
            OnDestroyPoolObject,
            true,
            10,
            50
        );
    }

    private void Start()
    {
        BuildDeck();
        DrawStartingHand();
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
        if (handCards.Count >= maxHandSize)
        {
            return;
        }

        if (drawPile.Count == 0)
        {
            ReshuffleDiscardPile();
        }

        if (drawPile.Count == 0)
        {
            Debug.LogWarning("[CardManager] No cards available in draw or discard pile!");
            return;
        }

        CardData card = drawPile[0];
        drawPile.RemoveAt(0);

        SpawnCardToHand(card);
    }

    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0)
        {
            Debug.LogWarning("[CardManager] Discard pile is empty, cannot reshuffle!");
            return;
        }

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();

        Debug.Log("[CardManager] Discard pile reshuffled into draw pile");
    }

    public void SpawnCardToHand(CardData dataToSpawn)
    {
        if (dataToSpawn == null)
        {
            Debug.LogError("[CardManager] Trying to spawn null CardData!");
            return;
        }

        CardDisplay spawnedCard = cardPool.Get();
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
        drawPile.Clear();
        discardPile.Clear();
        handCards.Clear();

        masterDeck.Clear();
        masterDeck.AddRange(deckList);

        drawPile.AddRange(masterDeck);

        ShuffleDrawPile();

        Debug.Log($"[CardManager] Deck built with {drawPile.Count} cards");
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
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawNextCard();
        }

        Debug.Log($"[CardManager] Drew starting hand with {handCards.Count} cards");
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

    private CardDisplay CreateNewCardInstance() => Instantiate(cardPrefab, handLayoutGroup);
    private void OnTakeCardFromPool(CardDisplay card) => card.gameObject.SetActive(true);
    private void OnReturnCardToPool(CardDisplay card) => card.gameObject.SetActive(false);
    private void OnDestroyPoolObject(CardDisplay card) => Destroy(card.gameObject);

    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public int HandCount => handCards.Count;
}