using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    [SerializeField] private float curveRadius = 500f;    
    [SerializeField] private float overlapRatio = 0.6f;     
    [SerializeField] private float cardWidth = 120f;        
    [SerializeField] private float layoutAnimDuration = 0.3f;
    [SerializeField] private Transform cardContainer;       

    private List<RectTransform> _cards = new List<RectTransform>();
    private static CardManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterCard(RectTransform cardRect)
    {
        if (!_cards.Contains(cardRect))
        {
            _cards.Add(cardRect);
            RefreshLayout();
        }
    }

    public void UnregisterCard(RectTransform cardRect)
    {
        if (_cards.Contains(cardRect))
        {
            _cards.Remove(cardRect);
            RefreshLayout();
        }
    }

    public void RefreshLayout()
    {
        int cardCount = _cards.Count;
        if (cardCount == 0) return;

        for (int i = 0; i < cardCount; i++)
        {
            Vector3 targetPos = CalculateCardPosition(i, cardCount);
            int targetSortingOrder = i; 

            RectTransform card = _cards[i];
            
            card.DOAnchorPos3D(targetPos, layoutAnimDuration)
                .SetEase(Ease.OutQuad);

            Canvas canvas = card.GetComponent<Canvas>();
            if (canvas) canvas.sortingOrder = targetSortingOrder;
        }
    }

    private Vector3 CalculateCardPosition(int index, int totalCards)
    {
       
        float spacing = cardWidth * (1f - overlapRatio);

        float totalWidth = (totalCards - 1) * spacing;
        float startX = -totalWidth * 0.5f;
        float xPos = startX + (index * spacing);

        float normalizedX = xPos / (curveRadius * 0.5f);
        normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
        
        float yPos = -(normalizedX * normalizedX) * 20f;

        return new Vector3(xPos, yPos, 0f);
    }

    public int GetCardIndex(RectTransform cardRect)
    {
        return _cards.IndexOf(cardRect);
    }
}