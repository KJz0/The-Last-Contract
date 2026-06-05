using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets; 
using UnityEngine.ResourceManagement.AsyncOperations; 

public class CardDisplay : MonoBehaviour
{
    [Header("Card Type & Name")]
    [SerializeField] private Image cardTypeIcon;
    [SerializeField] private TMP_Text cardNameText;

    [Header("Card Art")]
    [SerializeField] private Image cardArtImage;

    [Header("Cost Display (Top Left Corner)")]
    [SerializeField] private TMP_Text apCostText;
    [SerializeField] private TMP_Text manaCostText;
    
    public CardData CurrentCardData { get; private set; }
    private CardDraggable draggableComponent;

    private AsyncOperationHandle<Sprite> artLoadHandle;
    private AsyncOperationHandle<Sprite> iconLoadHandle;
    private bool isArtLoaded = false;
    private bool isIconLoaded = false;

    private void Awake()
    {
        draggableComponent = GetComponent<CardDraggable>();
    }

    public void Initialize(CardData data)
    {
        CurrentCardData = data;

        if (cardNameText != null) 
            cardNameText.text = data.cardName;
        if (apCostText != null) 
            apCostText.text = data.actionPointCost.ToString();
        if (manaCostText != null)
        {
            manaCostText.gameObject.SetActive(data.manaCost > 0);
            if (data.manaCost > 0)
                manaCostText.text = data.manaCost.ToString();
        }

        LoadCardTypeIconAsync(data.cardTypeIconReference);
        LoadCardArtAsync(data.cardArtReference);
        ResetCardVisualState();
    }

    // --- CARD TYPE ICON ---

    private void LoadCardTypeIconAsync(AssetReferenceSprite iconRef)
    {
        ReleaseSubscribedIcon();

        if (iconRef != null && iconRef.RuntimeKeyIsValid())
        {
            iconRef.LoadAssetAsync<Sprite>().Completed += OnIconLoaded;
        }
        else
        {
            if (cardTypeIcon != null) cardTypeIcon.sprite = null;
        }
    }

    private void OnIconLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            iconLoadHandle = handle;
            isIconLoaded = true;
            
            if (cardTypeIcon != null)
            {
                cardTypeIcon.sprite = handle.Result;
            }
        }
        else
        {
            Debug.LogError($"[Addressables] Gagal load icon untuk kartu: {CurrentCardData.cardName}");
        }
    }

    public void ReleaseSubscribedIcon()
    {
        if (isIconLoaded)
        {
            if (iconLoadHandle.IsValid())
            {
                Addressables.Release(iconLoadHandle);
            }
            isIconLoaded = false;
        }
    }

    // --- CARD ART ---

    private void LoadCardArtAsync(AssetReferenceSprite artRef)
    {
        ReleaseSubscribedArt();

        if (artRef != null && artRef.RuntimeKeyIsValid())
        {
            artRef.LoadAssetAsync<Sprite>().Completed += OnArtLoaded;
        }
        else
        {
            if (cardArtImage != null) cardArtImage.sprite = null;
        }
    }

    private void OnArtLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            artLoadHandle = handle;
            isArtLoaded = true;
            
            if (cardArtImage != null)
            {
                cardArtImage.sprite = handle.Result;
            }
        }
        else
        {
            Debug.LogError($"[Addressables] Gagal load gambar untuk kartu: {CurrentCardData.cardName}");
        }
    }

    public void ReleaseSubscribedArt()
    {
        if (isArtLoaded)
        {
            if (artLoadHandle.IsValid())
            {
                Addressables.Release(artLoadHandle);
            }
            isArtLoaded = false;
        }
    }

    // --- CLEANUP ---

    private void OnDisable()
    {
        ReleaseSubscribedArt();
        ReleaseSubscribedIcon();
    }

    private void ResetCardVisualState()
    {
        transform.localScale = Vector3.one;
        if (draggableComponent != null)
        {
            draggableComponent.ReleaseStickyLock();
        }
    }
}