using UnityEngine;
using UnityEngine.UI;
using TMPro;
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