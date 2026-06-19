using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Pool & Deck Settings")]
    [SerializeField] private CardDisplay    cardPrefab;
    [SerializeField] private Transform      handLayoutGroup;
    [SerializeField] private List<CardData> deckList = new();

    [SerializeField] private int maxHandSize = 6;

    [Header("Card Used Animation")]
    [Tooltip("Durasi animasi kartu menghilang saat dipakai ke target")]
    [SerializeField] private float useCardAnimDuration = 0.2f;

    [Header("Hand Refresh Animation (awal turn baru)")]
    [SerializeField] private float discardAnimDuration = 0.25f;
    [SerializeField] private float discardStagger      = 0.05f;
    [SerializeField] private float spawnAnimDuration   = 0.3f;
    [SerializeField] private float spawnStagger        = 0.08f;

    private readonly List<CardData>    masterDeck  = new();
    private IObjectPool<CardDisplay>   cardPool;
    private readonly List<CardData>    drawPile    = new();
    private readonly List<CardData>    discardPile = new();
    private readonly List<CardDisplay> handCards   = new();

    private HandManager handManager;
    private bool isRefreshingHand = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (cardPrefab == null)      { Debug.LogError("[CardManager] cardPrefab belum di-assign!"); return; }
        if (handLayoutGroup == null) { Debug.LogError("[CardManager] handLayoutGroup belum di-assign!"); return; }

        handManager = handLayoutGroup.GetComponent<HandManager>();

        cardPool = new ObjectPool<CardDisplay>(
            CreateNewCardInstance,
            OnTakeCardFromPool,
            OnReturnCardToPool,
            OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50);
    }

    private void Start()
    {
        if (deckList == null || deckList.Count == 0)
        {
            Debug.LogError("[CardManager] deckList kosong!");
            return;
        }

        BuildDeck();
        DrawStartingHand();
    }

    // ---------------------------------------------------------------
    // PUBLIC API
    // ---------------------------------------------------------------
    public bool UseCardOnTarget(CardDisplay card, Enemy targetEnemy)
    {
        if (card == null) { Debug.LogError("[CardManager] Card null"); return false; }

        CardData cardData = card.CurrentCardData;
        if (cardData == null) { Debug.LogError("[CardManager] CardData null"); return false; }

        if (PlayerManager.Instance == null) { Debug.LogError("[CardManager] PlayerManager null"); return false; }

        if (!PlayerManager.Instance.CanAffordAndSpend(cardData.actionPointCost, cardData.manaCost))
        {
            Debug.LogWarning($"[CardManager] Resource tidak cukup untuk {cardData.cardName}");
            return false;
        }

        Debug.Log($"[CardManager] Pakai kartu {cardData.cardName} ke {targetEnemy.name}");

        if (cardData.baseDamage > 0)
            targetEnemy.TakeDamage(cardData.baseDamage);

        if (cardData.effects != null)
        {
            foreach (CardEffect effect in cardData.effects)
            {
                if (effect == null) continue;
                effect.ExecuteEffect(targetEnemy, card);
            }
        }
        StartCoroutine(AnimateUsedCardThenDiscard(card));

        return true;
    }

    public void RefillHand()
    {
        while (handCards.Count < maxHandSize)
        {
            if (drawPile.Count == 0 && discardPile.Count == 0) break;
            DrawNextCard();
        }
    }

    public void SpawnCardToHand(CardData dataToSpawn)
    {
        if (dataToSpawn == null || cardPool == null) return;

        CardDisplay spawnedCard = cardPool.Get();
        if (spawnedCard == null) return;

        spawnedCard.transform.SetParent(handLayoutGroup, false);
        spawnedCard.Initialize(dataToSpawn);
        handCards.Add(spawnedCard);

        RefreshLayout();
    }
    public void RefreshHandWithAnimation()
    {
        if (isRefreshingHand)
        {
            Debug.LogWarning("[CardManager] RefreshHandWithAnimation sudah berjalan, diabaikan");
            return;
        }

        StartCoroutine(RefreshHandRoutine());
    }

    // ---------------------------------------------------------------
    // ANIMASI: KARTU DIPAKAI (ke player atau enemy)
    // ---------------------------------------------------------------
    private IEnumerator AnimateUsedCardThenDiscard(CardDisplay card)
    {
        if (card == null) yield break;

        Transform   cardTransform = card.transform;
        CanvasGroup cg            = card.GetComponent<CanvasGroup>();

        if (cg != null) cg.blocksRaycasts = false;

        Vector3 startScale = cardTransform.localScale;
        float   startAlpha = cg != null ? cg.alpha : 1f;
        float   elapsed    = 0f;

        while (elapsed < useCardAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / useCardAnimDuration;
            float easedT = t * t;

            cardTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, easedT);
            if (cg != null) cg.alpha  = Mathf.Lerp(startAlpha, 0f, easedT);

            yield return null;
        }

        cardTransform.localScale = Vector3.zero;
        if (cg != null) cg.alpha = 0f;
        DiscardCard(card);
    }

    // ---------------------------------------------------------------
    // HAND REFRESH ANIMATION (awal turn baru — satu-satunya tempat pooling)
    // ---------------------------------------------------------------

    private IEnumerator RefreshHandRoutine()
    {
        isRefreshingHand = true;

        List<CardDisplay> cardsToDiscard = new List<CardDisplay>(handCards);

        // FASE 1: kartu yang tersisa di tangan menghilang
        foreach (CardDisplay card in cardsToDiscard)
        {
            if (card == null) continue;
            StartCoroutine(AnimateCardOut(card));
            yield return new WaitForSeconds(discardStagger);
        }

        yield return new WaitForSeconds(discardAnimDuration);

        foreach (CardDisplay card in cardsToDiscard)
        {
            if (card == null) continue;
            DiscardCard(card);
        }

        // FASE 2: isi ulang tangan penuh — pooling terjadi di sini saja
        for (int i = 0; i < maxHandSize; i++)
        {
            if (drawPile.Count == 0) ReshuffleDiscardPile();
            if (drawPile.Count == 0) break;

            CardData data = drawPile[0];
            drawPile.RemoveAt(0);

            CardDisplay newCard = SpawnCardForAnimation(data);
            if (newCard != null)
                StartCoroutine(AnimateCardIn(newCard));

            yield return new WaitForSeconds(spawnStagger);
        }

        isRefreshingHand = false;
    }

    private CardDisplay SpawnCardForAnimation(CardData dataToSpawn)
    {
        if (dataToSpawn == null || cardPool == null) return null;

        CardDisplay spawnedCard = cardPool.Get();
        if (spawnedCard == null) return null;

        spawnedCard.transform.SetParent(handLayoutGroup, false);
        spawnedCard.Initialize(dataToSpawn);
        handCards.Add(spawnedCard);

        RefreshLayout();
        return spawnedCard;
    }

    private IEnumerator AnimateCardOut(CardDisplay card)
    {
        if (card == null) yield break;

        Transform   cardTransform = card.transform;
        CanvasGroup cg            = card.GetComponent<CanvasGroup>();

        if (cg != null) cg.blocksRaycasts = false;

        Vector3 startScale = cardTransform.localScale;
        float   startAlpha = cg != null ? cg.alpha : 1f;
        float   elapsed    = 0f;

        while (elapsed < discardAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / discardAnimDuration;
            float easedT = t * t;

            cardTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, easedT);
            if (cg != null) cg.alpha  = Mathf.Lerp(startAlpha, 0f, easedT);

            yield return null;
        }

        cardTransform.localScale = Vector3.zero;
        if (cg != null) cg.alpha = 0f;
    }

    private IEnumerator AnimateCardIn(CardDisplay card)
    {
        if (card == null) yield break;

        Transform   cardTransform = card.transform;
        CanvasGroup cg            = card.GetComponent<CanvasGroup>();

        cardTransform.localScale = Vector3.zero;
        if (cg != null) { cg.alpha = 0f; cg.blocksRaycasts = false; }

        float elapsed = 0f;

        while (elapsed < spawnAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / spawnAnimDuration;

            float overshoot = 1.15f;
            float easedT    = 1f + (overshoot - 1f) * Mathf.Sin(t * Mathf.PI);
            easedT = Mathf.Lerp(0f, easedT, t);

            cardTransform.localScale = Vector3.one * Mathf.Min(easedT, overshoot);
            if (cg != null) cg.alpha  = Mathf.Clamp01(t * 1.5f);

            yield return null;
        }

        cardTransform.localScale = Vector3.one;
        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }
    }

    // ---------------------------------------------------------------
    // PRIVATE: DRAW (dipakai internal oleh RefillHand / starting hand)
    // ---------------------------------------------------------------

    private void DrawNextCard()
    {
        if (handCards.Count >= maxHandSize) return;

        if (drawPile.Count == 0) ReshuffleDiscardPile();
        if (drawPile.Count == 0) { Debug.LogWarning("[CardManager] Tidak ada kartu"); return; }

        CardData card = drawPile[0];
        drawPile.RemoveAt(0);
        SpawnCardToHand(card);
    }

    // ---------------------------------------------------------------
    // DECK
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
        Debug.Log("[CardManager] Discard pile dikocok ulang");
    }

    private void ShuffleDrawPile()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
        }
    }

    private void DiscardCard(CardDisplay card)
    {
        if (card == null) return;

        CardData data = card.CurrentCardData;
        if (data != null && !data.isOneTimeUse)
        {
            discardPile.Add(data);
        }
        else if (data != null && data.isOneTimeUse)
        {
            Debug.Log($"[CardManager] {data.cardName} adalah one-time-use, tidak masuk discard pile");
        }

        handCards.Remove(card);

        card.transform.localScale = Vector3.one;
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }

        cardPool.Release(card);
        RefreshLayout();
    }
    public void ConsumeCardPermanently(CardDisplay card)
    {
        if (card == null) return;

        Debug.Log($"[CardManager] Mengkonsumsi kartu permanen: {card.CurrentCardData?.cardName}");

        StartCoroutine(AnimateConsumedCardThenRemove(card));
    }

    private IEnumerator AnimateConsumedCardThenRemove(CardDisplay card)
    {
        if (card == null) yield break;

        Transform   cardTransform = card.transform;
        CanvasGroup cg            = card.GetComponent<CanvasGroup>();

        if (cg != null) cg.blocksRaycasts = false;

        Vector3 startScale = cardTransform.localScale;
        float   startAlpha = cg != null ? cg.alpha : 1f;
        float   elapsed    = 0f;

        while (elapsed < useCardAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / useCardAnimDuration;
            float easedT = t * t;

            cardTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, easedT);
            if (cg != null) cg.alpha  = Mathf.Lerp(startAlpha, 0f, easedT);

            yield return null;
        }

        cardTransform.localScale = Vector3.zero;
        if (cg != null) cg.alpha = 0f;
        handCards.Remove(card);

        card.transform.localScale = Vector3.one;
        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }

        cardPool.Release(card);
        RefreshLayout();
    }
    public void AddCardToDrawPileTop(CardData newCard)
    {
        if (newCard == null) return;

        drawPile.Insert(0, newCard);
        Debug.Log($"[CardManager] {newCard.cardName} ditambahkan ke draw pile (posisi teratas)");
    }
    private void RefreshLayout()
    {
        handManager?.UpdateHandLayout();
    }

    // ---------------------------------------------------------------
    // POOL
    // ---------------------------------------------------------------

    private CardDisplay CreateNewCardInstance() =>
        (cardPrefab != null && handLayoutGroup != null)
            ? Instantiate(cardPrefab, handLayoutGroup)
            : null;

    private void OnTakeCardFromPool(CardDisplay card)
    {
        if (card == null) return;
        card.gameObject.SetActive(true);

        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true; }
    }

    private void OnReturnCardToPool(CardDisplay card)
    {
        if (card != null) card.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(CardDisplay card)
    {
        if (card != null) Destroy(card.gameObject);
    }

    public int DrawPileCount     => drawPile.Count;
    public int DiscardPileCount  => discardPile.Count;
    public int HandCount         => handCards.Count;
    public bool IsRefreshingHand => isRefreshingHand;

    [ContextMenu("Deck Debug")]
    private void DebugDeck() =>
        Debug.Log($"[CardManager] Draw:{drawPile.Count} Discard:{discardPile.Count} Hand:{handCards.Count}");
}