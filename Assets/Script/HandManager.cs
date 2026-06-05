using UnityEngine;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    [Header("Hand Curve Layout Settings")]
    [SerializeField] private float cardSpacing = 120f;   
    [SerializeField] private float curveIntensity = 15f;  
    [SerializeField] private float rotationIntensity = 5f; 
    [SerializeField] private float verticalOffset = -20f; 

    private List<CardDraggable> cardsInHand = new List<CardDraggable>();

    private void OnTransformChildrenChanged()
    {
        UpdateHandLayout();
    }

    private void Start()
    {
        UpdateHandLayout();
    }

    [ContextMenu("Refresh Layout")] 
    public void UpdateHandLayout()
    {
        CollectCardsInHand();
        LayoutCardsInCurve();
    }

    private void CollectCardsInHand()
    {
        cardsInHand.Clear();
        
        foreach (Transform child in transform)
        {
            CardDraggable card = child.GetComponent<CardDraggable>();
            if (card != null)
            {
                cardsInHand.Add(card);
            }
        }
    }

    private void LayoutCardsInCurve()
    {
        int totalCards = cardsInHand.Count;
        if (totalCards == 0) return;

        float centerIndex = (totalCards - 1) / 2f;

        for (int i = 0; i < totalCards; i++)
        {
            ApplyCardLayout(i, centerIndex);
        }
    }

    private void ApplyCardLayout(int cardIndex, float centerIndex)
    {
        float cardOffset = cardIndex - centerIndex;

        float posX = cardOffset * cardSpacing;
        float posY = -Mathf.Pow(cardOffset, 2) * curveIntensity + verticalOffset;
        float rotZ = -cardOffset * rotationIntensity;

        cardsInHand[cardIndex].homePosition = new Vector3(posX, posY, 0f);
        cardsInHand[cardIndex].homeRotation = Quaternion.Euler(0f, 0f, rotZ);
        cardsInHand[cardIndex].transform.SetSiblingIndex(cardIndex);
    }

    private void OnValidate()
    {
        UpdateHandLayout();
    }
}