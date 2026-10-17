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
    [Header("弹巢上半区：枪反杀伤弹")]
    [SerializeField] private Image[] lethalAmmoSlots = new Image[3];
    [Header("弹巢下半区：Dash 推进弹")]
    [SerializeField] private Image[] propulsionAmmoSlots = new Image[3];

    private void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 1;
            healthSlider.wholeNumbers = false;
            healthSlider.interactable = false;
        }
        if (reloadProgressBackground != null)
        {
            reloadBackgroundColor = reloadProgressBackground.color;
            hasReloadBackgroundColor = true;
        }
        observedReloadRewards = reloadProgress != null ? reloadProgress.RewardSequence : 0;
        RefreshHUD();
    }

    private void LateUpdate()
    {
        reloadFlashTimer = Mathf.Max(0, reloadFlashTimer - Time.deltaTime);
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (healthSlider != null)
            healthSlider.value = playerHealth != null ? playerHealth.NormalizedHealth : 0;

        SetAmmoSlots(lethalAmmoSlots, lethalAmmo != null ? lethalAmmo.CurrentAmmo : 0);
        int availableAmmo = propulsionAmmo != null ? propulsionAmmo.CurrentAmmo : 0;
        SetAmmoSlots(propulsionAmmoSlots, availableAmmo);
        RefreshReloadProgress();
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
}
