using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(
    fileName = "NewFusionRecipe",
    menuName  = "CardGame/Fusion/Fusion Recipe")]
public class FusionRecipe : ScriptableObject
{
    [Header("Kartu Bahan (kombinasi, urutan tidak penting)")]
    public CardData cardA;
    public CardData cardB;

    [Header("Pool Hasil per Rarity")]
    [Tooltip("Kartu yang bisa muncul jika hasil roll = Basic (50%)")]
    public List<CardData> basicResultPool = new();

    [Tooltip("Kartu yang bisa muncul jika hasil roll = Elite (15%)")]
    public List<CardData> eliteResultPool = new();

    [Tooltip("Kartu yang bisa muncul jika hasil roll = Epic (5%)")]
    public List<CardData> epicResultPool = new();

    public bool Matches(CardData inputA, CardData inputB)
    {
        if (inputA == null || inputB == null) return false;

        bool directMatch  = cardA == inputA && cardB == inputB;
        bool reverseMatch = cardA == inputB && cardB == inputA;

        return directMatch || reverseMatch;
    }
    public List<CardData> GetPoolForRarity(CardRarity rarity)
    {
        return rarity switch
        {
            CardRarity.Basic => basicResultPool,
            CardRarity.Elite => eliteResultPool,
            CardRarity.Epic  => epicResultPool,
            _                => null
        };
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        MarkPoolAsFusionResult(basicResultPool);
        MarkPoolAsFusionResult(eliteResultPool);
        MarkPoolAsFusionResult(epicResultPool);
    }

    private void MarkPoolAsFusionResult(List<CardData> pool)
    {
        if (pool == null) return;

        foreach (CardData card in pool)
        {
            if (card != null && !card.isFusionResult)
            {
                card.isFusionResult = true;
                UnityEditor.EditorUtility.SetDirty(card);
            }
        }
    }
#endif
}