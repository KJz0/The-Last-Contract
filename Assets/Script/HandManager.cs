using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float cardSpacing = 120f;
    [SerializeField] private float curveIntensity = 15f;
    [SerializeField] private float rotationIntensity = 5f;
    [SerializeField] private float verticalOffset = -20f;

    private void OnTransformChildrenChanged()
    {
        UpdateHandLayout();
    }

    public void UpdateHandLayout()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        int centerIndex = childCount / 2;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            CardDraggable draggable = child.GetComponent<CardDraggable>();

            if (draggable == null) continue;

            int offset = i - centerIndex;

            Vector3 pos = new Vector3(
                offset * cardSpacing,
                -offset * offset * curveIntensity + verticalOffset,
                0
            );

            float rotZ = -offset * rotationIntensity;

            child.localPosition = pos;
            child.localRotation = Quaternion.Euler(0, 0, rotZ);

            // Update home position & rotation
            draggable.HomePosition = pos;
            draggable.HomeRotation = Quaternion.Euler(0, 0, rotZ);
        }
    }
}