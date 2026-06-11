using UnityEngine;

/// <summary>
/// Mengatur layout kartu di tangan (kurva parabola).
/// OnTransformChildrenChanged otomatis di-trigger Unity saat kartu
/// ditambah atau dihapus dari hierarchy.
/// 
/// BUG FIX: setelah UpdateHandLayout, setiap CardDraggable harus
/// di-notify agar homePosition-nya mengikuti posisi layout baru.
/// Ini sudah ditangani lewat draggable.homePosition = pos (sudah benar sebelumnya),
/// tapi targetPosition di CardDraggable tidak di-sync jika kartu sedang idle.
/// Fix: panggil draggable.SnapToHome() agar posisi langsung terupdate.
/// </summary>
public class HandManager : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float cardSpacing      = 120f;
    [SerializeField] private float curveIntensity   = 15f;
    [SerializeField] private float rotationIntensity = 5f;
    [SerializeField] private float verticalOffset   = -20f;

    private void OnTransformChildrenChanged()
    {
        UpdateHandLayout();
    }

    public void UpdateHandLayout()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        // Gunakan childCount - 1 untuk menghindari overflow jika genap
        float centerIndex = (childCount - 1) / 2f;

        for (int i = 0; i < childCount; i++)
        {
            Transform      child     = transform.GetChild(i);
            CardDraggable  draggable = child.GetComponent<CardDraggable>();

            if (draggable == null) continue;

            float offset = i - centerIndex;

            Vector3 pos = new Vector3(
                offset * cardSpacing,
                -(offset * offset) * curveIntensity + verticalOffset,
                0
            );

            float rotZ = -offset * rotationIntensity;

            child.localPosition = pos;
            child.localRotation = Quaternion.Euler(0, 0, rotZ);

            // Update home position agar kartu snap kembali ke posisi baru
            draggable.homePosition = pos;
            draggable.homeRotation = Quaternion.Euler(0, 0, rotZ);
        }
    }
}