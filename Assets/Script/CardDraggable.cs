using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardDraggable : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IDragHandler,
    IEndDragHandler
{
    private CardDisplay   cardDisplay;
    private CanvasGroup   canvasGroup;
    private RectTransform rectTransform;

    [Header("Physics")]
    [SerializeField] private float moveSmoothSpeed  = 15f;
    [SerializeField] private float snapBackSpeed    = 12f;
    [SerializeField] private float maxTiltAngle     = 15f;
    [SerializeField] private float tiltSmoothSpeed  = 12f;
    [SerializeField] private float hoverScaleAmount = 1.15f;
    [SerializeField] private float hoverYOffset     = 40f;

    public Vector3    homePosition;
    public Quaternion homeRotation;

    private Vector3    targetPosition;
    private Quaternion targetRotation;

    private bool    isDragging = false;
    private bool    isHovered  = false;
    private Vector2 dragLocalPos;
    private bool    hasDragPos = false;

    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        cardDisplay   = GetComponent<CardDisplay>();
        canvasGroup   = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        homePosition   = transform.localPosition;
        homeRotation   = transform.localRotation;
        targetPosition = homePosition;
        targetRotation = homeRotation;
    }
    // ---------------------------------------------------------------
    // POINTER EVENTS
    // ---------------------------------------------------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;

        if (rectTransform != null && rectTransform.parent != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out dragLocalPos);

            hasDragPos = true;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        hasDragPos = false;

        // DEBUG
        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        Debug.Log($"[Drop] Total hit: {raycastResults.Count}");
        foreach (var r in raycastResults)
            Debug.Log($"  → {r.gameObject.name} | hasEnemy: {r.gameObject.GetComponent<Enemy>() != null} | parentEnemy: {r.gameObject.GetComponentInParent<Enemy>() != null}");

        Enemy target = FindEnemyUnderPointer(eventData);
        Debug.Log($"[Drop] Target found: {(target != null ? target.name : "NULL")}");

        if (target != null)
            CardManager.Instance?.UseCardOnTarget(cardDisplay, target);
    }

    // ---------------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------------

    private void Update()
    {
        HandleCardPhysics();
    }

    private void HandleCardPhysics()
    {
        if (isDragging && hasDragPos)
        {
            targetPosition = new Vector3(dragLocalPos.x, dragLocalPos.y, 0f);

            float normalized = Mathf.Clamp(
                (dragLocalPos.x - homePosition.x) / 200f, -1f, 1f);
            targetRotation = Quaternion.Euler(0, 0, -normalized * maxTiltAngle);

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one * hoverScaleAmount,
                Time.deltaTime * tiltSmoothSpeed);
        }
        else if (isHovered && !isDragging)
        {
            targetPosition = homePosition + Vector3.up * hoverYOffset;
            targetRotation = Quaternion.identity;

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one * hoverScaleAmount,
                Time.deltaTime * tiltSmoothSpeed);
        }
        else
        {
            targetPosition = homePosition;
            targetRotation = homeRotation;

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

    // ---------------------------------------------------------------
    // PUBLIC API
    // ---------------------------------------------------------------

    public void ReleaseStickyLock()
    {
        isHovered  = false;
        isDragging = false;
        hasDragPos = false;
    }

    // ---------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------

    private Enemy FindEnemyUnderPointer(PointerEventData eventData)
    {
        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject == gameObject) continue;
            if (result.gameObject.transform.IsChildOf(transform)) continue;

            if (result.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
                return enemy;

            Enemy enemyInParent = result.gameObject.GetComponentInParent<Enemy>();
            if (enemyInParent != null)
                return enemyInParent;
        }

        return null;
    }
}