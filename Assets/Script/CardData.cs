using UnityEngine;
using UnityEngine.AddressableAssets;

public enum CardType
{
    Attack,
    Magic,
    Buff
}

[CreateAssetMenu(fileName = "NewCardData", menuName = "CardGame/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    public string cardID;
    public string cardName;
    public CardType cardType = CardType.Attack;
    [TextArea(2, 5)] public string description;

    [Header("Resource Costs")]
    public int actionPointCost;
    public int manaCost = 0;

    [Header("Stats")]
    public int attackPower;

    [Header("Custom Effect (Optional)")]
    public CardEffect specialEffect;

    [Header("Visuals (Addressables)")]
    public AssetReferenceSprite cardArtReference;
    public AssetReferenceSprite cardTypeIconReference;
}