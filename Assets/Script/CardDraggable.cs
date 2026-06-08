using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDraggable : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IDragHandler,
    IEndDragHandler
{
    [Header("References")]
    [SerializeField] private GameObject tooltipPanel;
    private CardDisplay cardDisplay;
    private CanvasGroup canvasGroup;

    [Header("Physics")]
    [SerializeField] private float moveSmoothSpeed = 15f;
    [SerializeField] private float snapBackSpeed = 12f;
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSensitivity = 0.6f;
    [SerializeField] private float tiltSmoothSpeed = 12f;
    [SerializeField] private float hoverScaleAmount = 1.15f;
    [SerializeField] private float hoverYOffset = 40f;

    public Vector3 HomePosition { get; set; }
    public Quaternion HomeRotation { get; set; }
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private bool isDragging = false;
    private bool isHovered = false;
    private bool isClicked = false;
    private static CardDraggable activeStickyCard;
    private Vector3 dragOffset;

    private void Awake()
    {
        Debug.Log("[CardDraggable] Awake");
        cardDisplay = GetComponent<CardDisplay>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        HomePosition = transform.localPosition;
        HomeRotation = transform.localRotation;
        targetPosition = HomePosition;
        targetRotation = HomeRotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateCanvasSorting();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isClicked = !isClicked;

        if (isClicked)
        {
            activeStickyCard = this;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        dragOffset = (Vector3)localPoint;
        UpdateCanvasSorting();
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("[CardDraggable] OnEndDrag");
        isDragging = false;
        UpdateCanvasSorting();

        // 2D RAYCAST - Convert mouse position to world
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        
        Debug.Log($"[CardDraggable] Raycast from: {rayOrigin}");

        // Raycast 2D - small distance forward
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, 0.1f);

        if (hit.collider != null)
        {
            Debug.Log($"[CardDraggable] Raycast hit: {hit.collider.gameObject.name}");
            
            if (hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                Debug.Log($"[CardDraggable] Found enemy: {enemy.name}, using card");
                
                if (CardManager.Instance != null && cardDisplay != null)
                {
                    bool success = CardManager.Instance.UseCardOnTarget(cardDisplay, enemy);
                    Debug.Log($"[CardDraggable] UseCardOnTarget result: {success}");

                    if (!success)
                    {
                        SnapCardBack();
                    }
                }
                else
                {
                    Debug.LogError("[CardDraggable] CardManager or CardDisplay null!");
                    SnapCardBack();
                }
            }
            else
            {
                Debug.Log("[CardDraggable] Hit object is not an Enemy");
                SnapCardBack();
            }
        }
        else
        {
            Debug.Log("[CardDraggable] Raycast hit nothing");
            SnapCardBack();
        }
    }

    private void Update()
    {
        HandleCardPhysics();
    }

    private void HandleCardPhysics()
    {
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            worldMousePos.z = 0;

            targetPosition = worldMousePos + dragOffset;

            Vector3 directionToMouse = (worldMousePos - transform.position).normalized;
            float tiltZ = -directionToMouse.x * maxTiltAngle;
            targetRotation = Quaternion.Euler(0, 0, tiltZ);

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one * hoverScaleAmount,
                Time.deltaTime * tiltSmoothSpeed);
        }
        else if (isHovered && !isDragging)
        {
            targetPosition = HomePosition + Vector3.up * hoverYOffset;
            targetRotation = Quaternion.identity;
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one * hoverScaleAmount,
                Time.deltaTime * tiltSmoothSpeed);
        }
        else
        {
            targetPosition = HomePosition;
            targetRotation = HomeRotation;
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one,
                Time.deltaTime * tiltSmoothSpeed);
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * moveSmoothSpeed);

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * snapBackSpeed);
    }

    private void SnapCardBack()
    {
        isDragging = false;
        targetPosition = HomePosition;
        targetRotation = HomeRotation;
    }

    public void UpdateCanvasSorting()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isDragging ? 0.8f : 1f;
        }
    }

    public void ReleaseStickyLock()
    {
        if (activeStickyCard == this)
        {
            activeStickyCard = null;
            isClicked = false;
        }
    }
}