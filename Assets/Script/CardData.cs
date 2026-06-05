using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum CardType
{
    Attack,
    Magic,
    Buff
}

[CreateAssetMenu(
    fileName = "NewCardData",
    menuName = "CardGame/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    public string cardID;

    public string cardName;

    public CardType cardType;

    [TextArea]
    public string description;

    [Header("Cost")]
    public int actionPointCost;

    public int manaCost;

    [Header("Effects")]
    public List<CardEffect> effects = new();

    [Header("Visual")]
    public AssetReferenceSprite cardArtReference;

    public AssetReferenceSprite cardTypeIconReference;
}