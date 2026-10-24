using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 玩家屏幕 HUD：只读取战斗数据，不参与扣血或弹药消耗。
public class UI_PlayerHUD : MonoBehaviour
{
    [Header("数据来源")]
    [SerializeField] private Entity_Health playerHealth;
    [SerializeField] private Player_PropulsionAmmo propulsionAmmo;
    [SerializeField] private Player_LethalAmmo lethalAmmo;
    [SerializeField] private Player_ReloadProgress reloadProgress;
    [Header("近战装填蓝条")]
    [SerializeField] private Image reloadProgressFill;
    [SerializeField] private Image reloadProgressBackground;
    [SerializeField, Min(0)] private float reloadFlashDuration = 0.18f;
    private Color reloadBackgroundColor;
    private bool hasReloadBackgroundColor;
    private float reloadFlashTimer;
    private int observedReloadRewards;
    [Header("玩家血量")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Color healthLossColor = new Color(1f, 0.46f, 0.24f, 1f);
    [SerializeField, Min(0.01f)] private float healthLossDuration = 0.45f;
    [SerializeField, Min(0)] private float healthLossTravelDistance = 28f;
    [Header("弹巢上半区：枪反杀伤弹")]
    [SerializeField] private Image[] lethalAmmoSlots = new Image[3];
    [Header("弹巢下半区：Dash 推进弹")]
    [SerializeField] private Image[] propulsionAmmoSlots = new Image[3];

    private readonly List<HealthLossChip> healthLossChips = new List<HealthLossChip>();
    private RectTransform healthFillArea;
    private Image healthFillImage;
    private float observedHealth;

    private void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 1;
            healthSlider.wholeNumbers = false;
            healthSlider.interactable = false;

            if (healthSlider.fillRect != null)
            {
                healthFillArea = healthSlider.fillRect.parent as RectTransform;
                healthFillImage = healthSlider.fillRect.GetComponent<Image>();
            }
        }
        if (reloadProgressBackground != null)
        {
            reloadBackgroundColor = reloadProgressBackground.color;
            hasReloadBackgroundColor = true;
        }
        observedReloadRewards = reloadProgress != null ? reloadProgress.RewardSequence : 0;
        observedHealth = playerHealth != null ? playerHealth.NormalizedHealth : 0;
        RefreshHUD();
    }

    private void LateUpdate()
    {
        reloadFlashTimer = Mathf.Max(0, reloadFlashTimer - Time.deltaTime);
        UpdateHealthLossChips();
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (healthSlider != null)
        {
            float health = playerHealth != null ? playerHealth.NormalizedHealth : 0;
            if (health < observedHealth)
                SpawnHealthLossChip(observedHealth, health);

            observedHealth = health;
            healthSlider.value = health;
        }

        SetAmmoSlots(lethalAmmoSlots, lethalAmmo != null ? lethalAmmo.CurrentAmmo : 0);
        int availableAmmo = propulsionAmmo != null ? propulsionAmmo.CurrentAmmo : 0;
        SetAmmoSlots(propulsionAmmoSlots, availableAmmo);
        RefreshReloadProgress();
    }

    private void SpawnHealthLossChip(float previousHealth, float currentHealth)
    {
        if (healthFillArea == null)
            return;

        Rect area = healthFillArea.rect;
        float lostWidth = area.width * (previousHealth - currentHealth);
        if (lostWidth <= 0 || area.height <= 0)
            return;

        GameObject chipObject = new GameObject("Health Loss", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform chipRect = chipObject.GetComponent<RectTransform>();
        chipRect.SetParent(healthFillArea, false);
        chipRect.anchorMin = new Vector2(0, 1);
        chipRect.anchorMax = new Vector2(0, 1);
        chipRect.pivot = new Vector2(0, 0.5f);
        chipRect.anchoredPosition = new Vector2(area.width * currentHealth, -area.height * 0.5f);
        chipRect.sizeDelta = new Vector2(lostWidth, area.height);

        Image chipImage = chipObject.GetComponent<Image>();
        chipImage.sprite = healthFillImage != null ? healthFillImage.sprite : null;
        chipImage.type = Image.Type.Simple;
        chipImage.preserveAspect = false;
        chipImage.raycastTarget = false;
        chipImage.color = healthLossColor;
        chipRect.SetAsLastSibling();

        healthLossChips.Add(new HealthLossChip(chipRect, chipImage));
    }

    private void UpdateHealthLossChips()
    {
        for (int i = healthLossChips.Count - 1; i >= 0; i--)
        {
            HealthLossChip chip = healthLossChips[i];
            chip.Elapsed += Time.unscaledDeltaTime;
            float duration = Mathf.Max(0.01f, healthLossDuration);
            float progress = Mathf.Clamp01(chip.Elapsed / duration);
            float easedProgress = 1 - Mathf.Pow(1 - progress, 2);

            chip.Rect.anchoredPosition = chip.StartPosition +
                new Vector2(healthLossTravelDistance * easedProgress, healthLossTravelDistance * easedProgress);
            Color color = chip.StartColor;
            color.a *= 1 - progress;
            chip.Image.color = color;

            if (progress >= 1)
            {
                Destroy(chip.Rect.gameObject);
                healthLossChips.RemoveAt(i);
            }
        }
    }

    private void RefreshReloadProgress()
    {
        if (reloadProgressFill != null)
            reloadProgressFill.fillAmount = reloadProgress != null ? reloadProgress.NormalizedProgress : 0;
        int rewards = reloadProgress != null ? reloadProgress.RewardSequence : 0;
        if (rewards != observedReloadRewards)
        {
            observedReloadRewards = rewards;
            reloadFlashTimer = reloadProgress != null ? Mathf.Max(0, reloadFlashDuration) : 0;
        }
        if (reloadProgressBackground != null && hasReloadBackgroundColor)
        {
            Color color = reloadBackgroundColor;
            if (reloadFlashTimer > 0)
                color.a = 1;
            reloadProgressBackground.color = color;
        }
    }
    private void OnDisable()
    {
        reloadFlashTimer = 0;
        if (reloadProgressBackground != null && hasReloadBackgroundColor)
            reloadProgressBackground.color = reloadBackgroundColor;

        foreach (HealthLossChip chip in healthLossChips)
            if (chip.Rect != null)
                Destroy(chip.Rect.gameObject);
        healthLossChips.Clear();
    }

    private void SetAmmoSlots(Image[] slots, int count)
    {
        if (slots == null)
            return;

        count = Mathf.Clamp(count, 0, slots.Length);
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].enabled = i < count;
        }
    }

    private sealed class HealthLossChip
    {
        public readonly RectTransform Rect;
        public readonly Image Image;
        public readonly Vector2 StartPosition;
        public readonly Color StartColor;
        public float Elapsed;

        public HealthLossChip(RectTransform rect, Image image)
        {
            Rect = rect;
            Image = image;
            StartPosition = rect.anchoredPosition;
            StartColor = image.color;
        }
    }
}
