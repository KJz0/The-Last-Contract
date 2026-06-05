using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro; 

public class CardDraggable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static CardDraggable activeStickyCard;

    [Header("Movement Tuning")]
    [SerializeField] private float moveSmoothSpeed = 15f;
    [SerializeField] private float snapBackSpeed = 12f;

    [Header("Swing / Tilt Tuning")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSensitivity = 0.6f;
    [SerializeField] private float tiltSmoothSpeed = 12f;

    [Header("Tooltip Settings ")]
    [SerializeField] private GameObject tooltipPanel; 
    [SerializeField] private float hoverScaleAmount = 1.15f;
    [SerializeField] private float hoverYOffset = 40f; 

    [HideInInspector] public Vector3 homePosition;
    [HideInInspector] public Quaternion homeRotation = Quaternion.identity;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas cardCanvas; 
    private CardDisplay cardDisplay;
    private TMP_Text tooltipText;

    private bool isDragging = false;
    private bool isHovered = false;
    private bool isClicked = false; 
    private Vector3 dragOffset;
    private Vector3 lastMousePosition;
    private float currentTiltZ = 0f;

    private void Awake()
    {
        InitializeComponents();
        InitializeCanvasForCard();
        InitializeTooltip();
        StoreHomePosition();
    }

    private void Start()
    {
        UpdateCanvasSorting();
    }

    private void Update()
    {
        HandleTooltipStickyLogic(); 
        HandleCardPhysics();
    }

    // --- INITIALIZATION ---

    private void InitializeComponents()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cardDisplay = GetComponent<CardDisplay>();
    }

    private void InitializeCanvasForCard()
    {
        cardCanvas = GetComponent<Canvas>();
        if (cardCanvas == null)
        {
            cardCanvas = gameObject.AddComponent<Canvas>();
        }
        cardCanvas.overrideSorting = true;
        cardCanvas.sortingOrder = 0;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void InitializeTooltip()
    {
        if (tooltipPanel != null) 
        {
            tooltipText = tooltipPanel.GetComponentInChildren<TMP_Text>();
            tooltipPanel.SetActive(false); 
        }
    }

    private void StoreHomePosition()
    {
        homePosition = rectTransform.localPosition;
        homeRotation = rectTransform.localRotation;
    }

    // --- TOOLTIP & INTERACTION LOGIC ---

    private void HandleTooltipStickyLogic()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!isHovered && isClicked)
            {
                ReleaseStickyLock();
                if (activeStickyCard == this) activeStickyCard = null;
            }
        }

        bool shouldShowTooltip = isHovered || isClicked || isDragging;
        UpdateTooltipDisplay(shouldShowTooltip);
    }

    private void UpdateTooltipDisplay(bool shouldShow)
    {
        if (tooltipPanel == null || tooltipPanel.activeSelf == shouldShow) return;

        if (shouldShow && tooltipText != null && cardDisplay != null && cardDisplay.CurrentCardData != null)
        {
            BuildTooltipText();
        }

        tooltipPanel.SetActive(shouldShow);
    }

    private void BuildTooltipText()
    {
        CardData cardData = cardDisplay.CurrentCardData;
        string tooltipContent = cardData.description;
        tooltipText.text = tooltipContent;
    }

    // --- CARD PHYSICS ---

    private void HandleCardPhysics()
    {
        Vector3 currentMousePos = GetCurrentMousePosition();

        if (isDragging)
        {
            UpdateDraggingPosition(currentMousePos);
        }
        else
        {
            UpdateRestingPosition();
        }

        UpdateCardScale();
        lastMousePosition = currentMousePos;
    }

    private void UpdateDraggingPosition(Vector3 currentMousePos)
    {
        Vector3 targetPosition = currentMousePos + dragOffset;
        rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, Time.deltaTime * moveSmoothSpeed);

        float mouseDeltaX = currentMousePos.x - lastMousePosition.x;
        float targetTilt = -mouseDeltaX * tiltSensitivity;
        targetTilt = Mathf.Clamp(targetTilt, -maxTiltAngle, maxTiltAngle);
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTilt, Time.deltaTime * tiltSmoothSpeed);
        
        rectTransform.localRotation = Quaternion.Euler(0, 0, currentTiltZ);
    }

    private void UpdateRestingPosition()
    {
        ValidateHomeRotation();

        Vector3 targetLocalPosition = homePosition;
        if (isHovered || isClicked)
        {
            targetLocalPosition += new Vector3(0, hoverYOffset, 0);
        }

        rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetLocalPosition, Time.deltaTime * snapBackSpeed);
        rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, homeRotation, Time.deltaTime * snapBackSpeed);
        
        currentTiltZ = rectTransform.localRotation.eulerAngles.z;
        if (currentTiltZ > 180) currentTiltZ -= 360f;
    }

    private void UpdateCardScale()
    {
        float targetScale = (isHovered || isClicked) && !isDragging ? hoverScaleAmount : 1f;
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * targetScale, Time.deltaTime * 10f);
    }

    private void ValidateHomeRotation()
    {
        if (homeRotation.x == 0f && homeRotation.y == 0f && homeRotation.z == 0f && homeRotation.w == 0f)
        {
            homeRotation = Quaternion.identity; 
        }
    }

    // --- CANVAS SORTING ---

    public void UpdateCanvasSorting()
    {
        if (cardCanvas != null)
        {
            int baseSortingOrder = transform.GetSiblingIndex();
            cardCanvas.sortingOrder = (isHovered || isDragging || isClicked) ? 100 : baseSortingOrder;
        }
    }

    public void ReleaseStickyLock()
    {
        isClicked = false;
        UpdateCanvasSorting();
    }

    // --- POINTER EVENTS ---

    public void OnPointerDown(PointerEventData eventData)
    {
        isClicked = !isClicked;
        HandleStickyCardLogic();
        UpdateCanvasSorting();

        Vector3 currentMousePos = GetCurrentMousePosition();
        dragOffset = rectTransform.position - currentMousePos;
        lastMousePosition = currentMousePos;
    }

    private void HandleStickyCardLogic()
    {
        if (isClicked)
        {
            if (activeStickyCard != null && activeStickyCard != this)
            {
                activeStickyCard.ReleaseStickyLock();
            }
            activeStickyCard = this; 
        }
        else
        {
            if (activeStickyCard == this)
            {
                activeStickyCard = null; 
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        UpdateCanvasSorting(); 
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false; 
        UpdateCanvasSorting(); 
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        TryUseCardOnTarget(eventData);
    }

    private void TryUseCardOnTarget(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;
            Enemy enemyTarget = hitObject.GetComponentInParent<Enemy>();

            if (enemyTarget != null)
            {
                CardDisplay thisCardDisplay = GetComponent<CardDisplay>();
                bool isSuccess = CardManager.Instance.UseCardOnTarget(thisCardDisplay, enemyTarget);
                if (isSuccess) return;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (activeStickyCard != null && activeStickyCard != this)
        {
            activeStickyCard.ReleaseStickyLock();
            activeStickyCard = null; 
        }

        UpdateCanvasSorting();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateCanvasSorting(); 
    }

    // --- UTILITY ---

    private Vector3 GetCurrentMousePosition()
    {
        if (Mouse.current != null) return Mouse.current.position.ReadValue();
        if (Pointer.current != null) return Pointer.current.position.ReadValue();
        return Vector3.zero;
    }

    private void OnDisable()
    {
        if (activeStickyCard == this)
        {
            activeStickyCard = null;
        }
    }
}