using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengatur layout kartu di tangan (kurva parabola), dinamis menyesuaikan
/// jumlah kartu yang AKTIF saja — kartu yang sudah di-pool (SetActive(false))
/// tidak dihitung dalam layout, sehingga sisa kartu otomatis reflow
/// mengisi ruang yang ditinggalkan kartu yang dipakai/dibuang.
/// </summary>
public class HandManager : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float cardSpacing       = 120f;
    [SerializeField] private float curveIntensity    = 15f;
    [SerializeField] private float rotationIntensity = 5f;
    [SerializeField] private float verticalOffset    = -20f;

    [Header("Layout Transition")]
    [Tooltip("Kecepatan kartu lain bergeser mengisi slot kosong saat layout berubah")]
    [SerializeField] private float reflowSmoothSpeed = 10f;

    private readonly List<CardDraggable> trackedCards = new();

    private void OnTransformChildrenChanged()
    {
        UpdateHandLayout();
    }

    /// <summary>
    /// Hitung ulang posisi kurva hanya untuk kartu yang sedang aktif.
    /// Dipanggil otomatis saat child berubah, dan juga dipanggil manual
    /// oleh CardManager setiap kali kartu dipakai/dibuang/di-spawn
    /// agar layout selalu sinkron meskipun OnTransformChildrenChanged
    /// tidak terpicu (misalnya saat hanya SetActive tanpa reparenting).
    /// </summary>
    public void UpdateHandLayout()
    {
        // Kumpulkan hanya child yang aktif — kartu yang di-pool (inactive)
        // tidak ikut dihitung dalam kurva.
        trackedCards.Clear();

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            CardDraggable draggable = child.GetComponent<CardDraggable>();
            if (draggable == null) continue;

            trackedCards.Add(draggable);
        }

        int activeCount = trackedCards.Count;
        if (activeCount == 0) return;

        float centerIndex = (activeCount - 1) / 2f;

        for (int i = 0; i < activeCount; i++)
        {
            CardDraggable draggable = trackedCards[i];
            float         offset    = i - centerIndex;

            Vector3 pos = new Vector3(
                offset * cardSpacing,
                -(offset * offset) * curveIntensity + verticalOffset,
                0
            );

            float rotZ = -offset * rotationIntensity;
            draggable.homePosition = pos;
            draggable.homeRotation = Quaternion.Euler(0, 0, rotZ);
        }
    }
}