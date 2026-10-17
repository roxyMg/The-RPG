using UnityEngine;

// 杀伤弹独立弹仓：非处决击杀累计补弹，处决击杀直接补弹，不自动补弹。
public class Player_LethalAmmo : MonoBehaviour
{
    [Header("杀伤弹容量")]
    [SerializeField, Min(1)] private int capacity = 3;
    [SerializeField] private int currentAmmo;
    [Header("非处决击杀补弹")]
    [SerializeField, Min(1)] private int killsPerRound = 3;
    [Tooltip("当前累计进度；开局归零，处决击杀不增加也不清空此进度。")]
    [SerializeField] private int killProgress;
    public int KillsPerRound => Mathf.Max(1, killsPerRound);
    public int KillProgress => killProgress;
    public int CurrentAmmo => currentAmmo;
    public int Capacity => capacity;
    private void Awake()
    {
        currentAmmo = capacity;
        killProgress = 0;
    }
    public bool TryConsume()
    {
        if (!isActiveAndEnabled || currentAmmo <= 0)
            return false;
        currentAmmo--;
        return true;
    }
    // Enemy_Health 只在敌人由存活转为死亡时调用一次。
    public void RecordKill(bool isExecutionKill)
    {
        if (isExecutionKill)
        {
            Restore(1);
            return;
        }
        killProgress++;
        if (killProgress >= KillsPerRound)
        {
            // 满仓也消耗本次奖励，不储存超出容量的弹药。
            killProgress = 0;
            Restore(1);
        }
    }
    public void Restore(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + Mathf.Max(0, amount), 0, capacity);
    }
    private void OnValidate()
    {
        capacity = Mathf.Max(1, capacity);
        currentAmmo = Mathf.Clamp(currentAmmo, 0, capacity);
        killsPerRound = Mathf.Max(1, killsPerRound);
        killProgress = Mathf.Clamp(killProgress, 0, KillsPerRound - 1);
    }
}
