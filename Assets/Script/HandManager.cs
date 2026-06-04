using UnityEngine;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    [Header("Hand Curve Layout Settings")]
    [SerializeField] private float cardSpacing = 120f;     // Jarak horizontal antar kartu
    [SerializeField] private float curveIntensity = 15f;   // Seberapa melengkung ke bawah kartu di pinggir
    [SerializeField] private float rotationIntensity = 5f; // Seberapa miring kartu di pinggir (kipas)
    [SerializeField] private float verticalOffset = -20f;  // Dorong keseluruhan lengkungan ke atas/bawah

    private List<CardDraggable> cardsInHand = new List<CardDraggable>();

    private void OnTransformChildrenChanged()
    {
        // Fungsi ini sekarang HANYA bakal jalan kalau beneran ada kartu baru ditarik (Draw) atau kartu dibuang (Play)
        UpdateHandLayout();
    }

    private void Start()
    {
        UpdateHandLayout();
    }

    [ContextMenu("Refresh Layout")] 
    public void UpdateHandLayout()
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

        int totalCards = cardsInHand.Count;
        if (totalCards == 0) return;

        float centerIndex = (totalCards - 1) / 2f;

        for (int i = 0; i < totalCards; i++)
        {
            float cardOffset = i - centerIndex;

            float posX = cardOffset * cardSpacing;
            float posY = -Mathf.Pow(cardOffset, 2) * curveIntensity + verticalOffset;
            float rotZ = -cardOffset * rotationIntensity;

            cardsInHand[i].homePosition = new Vector3(posX, posY, 0f);
            cardsInHand[i].homeRotation = Quaternion.Euler(0f, 0f, rotZ);
            
            // Atur sibling index bawaan biar susunan numpuknya rapi secara default
            cardsInHand[i].transform.SetSiblingIndex(i);
        }
    }

    private void OnValidate()
    {
        UpdateHandLayout();
    }
}