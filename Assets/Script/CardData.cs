using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum CardType
{
    Attack,
    Magic,
    Buff
}

/// <summary>
/// Data kartu. Buat satu asset per kartu di Project window.
/// </summary>
[CreateAssetMenu(
    fileName = "NewCardData",
    menuName  = "CardGame/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    public string   cardID;
    public string   cardName;
    public CardType cardType;

    [TextArea]
    public string description;

    [Header("Cost")]
    public int actionPointCost;
    public int manaCost;

    [Header("Quick Reference (for UI display)")]
    public int attackPower;

    [Header("Effects")]
    public List<CardEffect> effects = new();

    [Header("Visual")]
    public AssetReferenceSprite cardArtReference;
    public AssetReferenceSprite cardTypeIconReference;
}