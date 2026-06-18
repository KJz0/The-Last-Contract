using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengontrol tampilan orb AP dan Mana berbentuk bulat.
///
/// AP: Image dengan Image Type = Filled, Fill Method = Radial 360,
///     tanpa efek apapun — hanya fillAmount yang berubah.
///
/// Mana: Image dengan material custom "CardGame/UI/ManaWaterFill",
///       permukaan air bergelombang secara otomatis lewat shader,
///       script ini hanya mengatur _FillAmount.
///
/// Setup di Inspector:
/// 1. Buat GameObject lingkaran untuk AP, Image Type = Filled,
///    Fill Method = Radial 360, assign ke field apOrbImage.
/// 2. Buat GameObject lingkaran untuk Mana, Image Type = Simple,
///    buat Material baru dengan Shader "CardGame/UI/ManaWaterFill",
///    assign Material itu ke Image, lalu assign Image ke manaOrbImage.
/// </summary>
public class ResourceOrbDisplay : MonoBehaviour
{
    [Header("AP Orb (bulat polos, tanpa animasi)")]
    [SerializeField] private Image    apOrbImage;
    [SerializeField] private TMP_Text apOrbText;

    [Header("Mana Orb (gelombang air, pakai shader)")]
    [SerializeField] private Image    manaOrbImage;
    [SerializeField] private TMP_Text manaOrbText;

    [Header("Animasi transisi nilai")]
    [Tooltip("Kecepatan transisi fill amount saat resource berubah")]
    [SerializeField] private float fillTransitionSpeed = 6f;

    private float apFillTarget   = 1f;
    private float manaFillTarget = 1f;

    private static readonly int FillAmountID = Shader.PropertyToID("_FillAmount");
    private MaterialPropertyBlock manaPropertyBlock;

    private void Awake()
    {
        if (apOrbImage != null)
            apOrbImage.type = Image.Type.Filled;
        if (manaOrbImage != null && manaOrbImage.material != null)
            manaOrbImage.material = Instantiate(manaOrbImage.material);
    }

    private void Update()
    {
        AnimateApOrb();
        AnimateManaOrb();
    }
    // ---------------------------------------------------------------
    // PUBLIC API
    // ---------------------------------------------------------------
    public void SetAP(int current, int max)
    {
        apFillTarget = max > 0 ? (float)current / max : 0f;

        if (apOrbText != null)
            apOrbText.text = $"{current}/{max}";
    }
    public void SetMana(int current, int max)
    {
        manaFillTarget = max > 0 ? (float)current / max : 0f;

        if (manaOrbText != null)
            manaOrbText.text = $"{current}/{max}";
    }

    // ---------------------------------------------------------------
    // PRIVATE
    // ---------------------------------------------------------------

    private void AnimateApOrb()
    {
        if (apOrbImage == null) return;
        apOrbImage.fillAmount = Mathf.Lerp(
            apOrbImage.fillAmount,
            apFillTarget,
            Time.deltaTime * fillTransitionSpeed);
    }

    private void AnimateManaOrb()
    {
        if (manaOrbImage == null || manaOrbImage.material == null) return;
        float currentFill = manaOrbImage.material.GetFloat(FillAmountID);
        float newFill = Mathf.Lerp(
            currentFill,
            manaFillTarget,
            Time.deltaTime * fillTransitionSpeed);

        manaOrbImage.material.SetFloat(FillAmountID, newFill);
    }
}