using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardDraggable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ====================================================================
    // GLOBAL MEDIATOR: Menyimpan data kartu mana yang SEKARANG lagi dikunci infonya.
    // Variabel static ini di-share dan diakses oleh semua kartu di game lu.
    // ====================================================================
    public static CardDraggable activeStickyCard;

    [Header("Movement Tuning")]
    [SerializeField] private float moveSmoothSpeed = 15f;
    [SerializeField] private float snapBackSpeed = 12f;

    [Header("Swing / Tilt Tuning")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSensitivity = 0.6f;
    [SerializeField] private float tiltSmoothSpeed = 12f;

    [Header("UI References (Hover Info)")]
    [SerializeField] private GameObject tooltipObject; 
    [SerializeField] private float hoverScaleAmount = 1.15f; 

    [Header("Hover Settings")]
    [SerializeField] private float hoverYOffset = 40f; 

    [HideInInspector] public Vector3 homePosition;
    [HideInInspector] public Quaternion homeRotation = Quaternion.identity;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas cardCanvas; 

    private bool isDragging = false;
    private bool isHovered = false;
    private bool isClicked = false; 
    
    private Vector3 dragOffset;
    private Vector3 lastMousePosition;
    private float currentTiltZ = 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
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

        homePosition = rectTransform.localPosition;
        homeRotation = rectTransform.localRotation;

        if (tooltipObject != null) tooltipObject.SetActive(false); 
    }

    private void Update()
    {
        HandleTooltipStickyLogic(); 
        HandleCardPhysics();
    }

    private void HandleTooltipStickyLogic()
    {
        // Deteksi klik kiri di layar (klik tempat kosong buat nutup kuncian)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!isHovered && isClicked)
            {
                ReleaseStickyLock();
                if (activeStickyCard == this) activeStickyCard = null;
            }
        }

        bool shouldShowTooltip = isHovered || isClicked || isDragging;

        if (tooltipObject != null && tooltipObject.activeSelf != shouldShowTooltip)
        {
            tooltipObject.SetActive(shouldShowTooltip);
        }
    }

    private void HandleCardPhysics()
    {
        Vector3 currentMousePos = GetCurrentMousePosition();

        if (isDragging)
        {
            Vector3 targetPosition = currentMousePos + dragOffset;
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, Time.deltaTime * moveSmoothSpeed);

            float mouseDeltaX = currentMousePos.x - lastMousePosition.x;
            float targetTilt = -mouseDeltaX * tiltSensitivity;
            targetTilt = Mathf.Clamp(targetTilt, -maxTiltAngle, maxTiltAngle);
            currentTiltZ = Mathf.Lerp(currentTiltZ, targetTilt, Time.deltaTime * tiltSmoothSpeed);
            
            rectTransform.localRotation = Quaternion.Euler(0, 0, currentTiltZ);
        }
        else
        {
            if (homeRotation.x == 0f && homeRotation.y == 0f && homeRotation.z == 0f && homeRotation.w == 0f)
            {
                homeRotation = Quaternion.identity; 
            }

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

        float targetScale = (isHovered || isClicked) && !isDragging ? hoverScaleAmount : 1f;
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * targetScale, Time.deltaTime * 10f);

        lastMousePosition = currentMousePos;
    }

    private void UpdateCanvasSorting()
    {
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = (isHovered || isDragging || isClicked) ? 100 : 0;
        }
    }

    // ====================================================================
    // FUNGSI MANDIRI: Buat ngelepas status kuncian kartu & benerin sorting-nya
    // ====================================================================
    public void ReleaseStickyLock()
    {
        isClicked = false;
        UpdateCanvasSorting();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isClicked = !isClicked; 

        if (isClicked)
        {
            // Jika ada kartu LAIN yang lagi kekunci, paksa dia lepas dulu sebelum kita ganti posisi
            if (activeStickyCard != null && activeStickyCard != this)
            {
                activeStickyCard.ReleaseStickyLock();
            }
            activeStickyCard = this; // Daftarkan kartu ini sebagai pemegang kuncian global baru
        }
        else
        {
            if (activeStickyCard == this)
            {
                activeStickyCard = null; // Hapus pendaftaran kalau statusnya di-unclick manual
            }
        }

        UpdateCanvasSorting();

        Vector3 currentMousePos = GetCurrentMousePosition();
        dragOffset = rectTransform.position - currentMousePos;
        lastMousePosition = currentMousePos;
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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        // ====================================================================
        // MEKANIK BARU: Deteksi Kartu Lain Saat Di-hover
        // Jika mouse masuk ke kartu ini, dan ada kartu lain yang lagi kuncian (Sticky),
        // paksa kartu lama itu buat langsung lepas kuncian dan turun!
        // ====================================================================
        if (activeStickyCard != null && activeStickyCard != this)
        {
            activeStickyCard.ReleaseStickyLock();
            activeStickyCard = null; // Reset mediator karena kuncian lama udah buyar
        }

        UpdateCanvasSorting();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateCanvasSorting(); 
    }

    // Pengaman memory leak: kalau kartu dihancurkan/di-disable pas lagi kekunci, reset static pointer-nya
    private void OnDisable()
    {
        if (activeStickyCard == this)
        {
            activeStickyCard = null;
        }
    }

    private Vector3 GetCurrentMousePosition()
    {
        if (Mouse.current != null) return Mouse.current.position.ReadValue();
        if (Pointer.current != null) return Pointer.current.position.ReadValue();
        return Vector3.zero;
    }
}