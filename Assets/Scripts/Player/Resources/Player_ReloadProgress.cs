using UnityEngine;

// 近战装填进度独立于击杀计数；HUD 只观察，弹药仍由弹仓管理。
[RequireComponent(typeof(Player_LethalAmmo))]
public class Player_ReloadProgress : MonoBehaviour
{
    public const float ProgressRequired = 100f;
    [SerializeField] private Player_LethalAmmo lethalAmmo;
    [SerializeField, Min(0)] private float reloadProgress;
    public float ReloadProgress => reloadProgress;
    public float NormalizedProgress => Mathf.Clamp01(reloadProgress / ProgressRequired);
    // 只在装填进度实际补弹时递增，用于 HUD 提示；击杀补弹不触发它。
    public int RewardSequence { get; private set; }

    private void Awake()
    {
        if (lethalAmmo == null)
            lethalAmmo = GetComponent<Player_LethalAmmo>();
        reloadProgress = 0;
        RewardSequence = 0;
    }
    public void AddProgress(float amount)
    {
        if (!isActiveAndEnabled || lethalAmmo == null || amount <= 0 ||
            float.IsNaN(amount) || float.IsInfinity(amount) ||
            lethalAmmo.CurrentAmmo >= lethalAmmo.Capacity)
            return;
        reloadProgress += amount;
        // 大数值也逐发结算；填满弹仓时保留已获得的余量。
        while (reloadProgress >= ProgressRequired && lethalAmmo.CurrentAmmo < lethalAmmo.Capacity)
        {
            reloadProgress -= ProgressRequired;
            lethalAmmo.Restore(1);
            RewardSequence++;
        }
    }
    private void OnValidate()
    {
        reloadProgress = float.IsNaN(reloadProgress) || float.IsInfinity(reloadProgress)
            ? 0 : Mathf.Max(0, reloadProgress);
    }
}
