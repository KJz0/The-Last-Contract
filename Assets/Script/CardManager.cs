using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using TMPro;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Pool & Deck Settings")]
    [SerializeField] private CardDisplay cardPrefab;
    [SerializeField] private Transform handLayoutGroup;
    [SerializeField] private List<CardData> deckList = new List<CardData>(); 

    [Header("Action Point (AP) System")]
    [SerializeField] private int maxActionPoints = 8;
    [SerializeField] private TMP_Text playerAPText; 
    
    private int currentActionPoints;
    private IObjectPool<CardDisplay> cardPool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cardPool = new ObjectPool<CardDisplay>(
            CreateNewCardInstance, OnTakeCardFromPool, OnReturnCardToPool, OnDestroyPoolObject,
            true, 10, 50
        );
    }

    private void Start()
    {
        ResetActionPoints();

        for (int i = 0; i < 4; i++)
        {
            DrawNextCard();
        }
    }

    public bool UseCardOnTarget(CardDisplay card, Enemy targetEnemy)
    {
        CardData cardData = card.CurrentCardData;

        if (!PlayerManager.Instance.CanAffordAndSpend(cardData.actionPointCost, cardData.manaCost))
        {
            Debug.LogWarning($"[Combat Info] Resource lu gak cukup buat mainin {cardData.cardName}!");
            return false;
        }

        if (cardData.attackPower > 0)
        {
            targetEnemy.TakeDamage(cardData.attackPower);
        }

        if (cardData.specialEffect != null)
        {
            cardData.specialEffect.ExecuteEffect(targetEnemy, card);
        }

        currentActionPoints -= cardData.actionPointCost;
        UpdateAPUI();

        Debug.Log($"[Combat Loop] Kartu {cardData.cardName} sukses digunakan. Sisa AP: {currentActionPoints}");

        DespawnCard(card);
        DrawNextCard();

        return true;
    }

    public void ResetActionPoints()
    {
        currentActionPoints = maxActionPoints;
        UpdateAPUI();
    }

    private void UpdateAPUI()
    {
        if (playerAPText != null)
        {
            playerAPText.text = $"AP: {currentActionPoints}/{maxActionPoints}";
        }
    }

    public void DrawNextCard()
    {
        if (deckList.Count > 0)
        {
            CardData nextCardData = deckList[Random.Range(0, deckList.Count)];
            SpawnCardToHand(nextCardData);
        }
    }

    public void SpawnCardToHand(CardData dataToSpawn)
    {
        CardDisplay spawnedCard = cardPool.Get();
        spawnedCard.transform.SetParent(handLayoutGroup, false);
        spawnedCard.Initialize(dataToSpawn);
        if (spawnedCard.TryGetComponent<CardDraggable>(out var drag)) 
        {
            drag.UpdateCanvasSorting();
        }
    }

    public void DespawnCard(CardDisplay cardToDiscard) => cardPool.Release(cardToDiscard);

    private CardDisplay CreateNewCardInstance() => Instantiate(cardPrefab, handLayoutGroup);
    private void OnTakeCardFromPool(CardDisplay card) => card.gameObject.SetActive(true);
    private void OnReturnCardToPool(CardDisplay card) => card.gameObject.SetActive(false);
    private void OnDestroyPoolObject(CardDisplay card) => Destroy(card.gameObject);
}