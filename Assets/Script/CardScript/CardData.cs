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

    [Header("Damage")]
    [Tooltip("Damage langsung ke enemy sebelum effects dijalankan. 0 = tidak ada base damage.")]
    public int baseDamage;

    [Header("Effects")]
    public List<CardEffect> effects = new();

    [Header("Visual")]
    public AssetReferenceSprite cardArtReference;
    public AssetReferenceSprite cardTypeIconReference;

    [Header("Fusion")]
    [Tooltip("Rarity hasil fusion. Biarkan None untuk kartu basic biasa yang bukan hasil fusion.")]
    public CardRarity fusionRarity = CardRarity.None;

    [Tooltip("Jika true, kartu ini hanya bisa dipakai sekali lalu hilang permanen dari deck (tidak masuk discard pile untuk di-reshuffle). Biasanya dicentang untuk kartu hasil fusion.")]
    public bool isOneTimeUse = false;

    [Tooltip("Centang jika kartu ini adalah HASIL dari fusion. Kartu hasil fusion tidak bisa dipakai sebagai bahan fusion lagi. Dicentang otomatis untuk kartu yang ditambahkan ke pool hasil recipe — pastikan ini true di semua CardData yang masuk ke basicResultPool/eliteResultPool/epicResultPool.")]
    public bool isFusionResult = false;

    public bool CannotBeFused => isFusionResult || fusionRarity != CardRarity.None;
}