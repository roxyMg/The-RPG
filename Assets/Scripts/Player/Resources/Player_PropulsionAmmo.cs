using UnityEngine;

// 推进弹独立弹仓：每次推进消耗一发，按游戏时间逐发补充。
public class Player_PropulsionAmmo : MonoBehaviour
{
    [Header("推进弹容量")]
    [SerializeField, Min(1)] private int capacity = 3;
    [Header("自动补弹")]
    [SerializeField, Min(.01f)] private float refillInterval = 1.5f;
    [Header("运行时弹量：进入游戏时装满")]
    [SerializeField] private int currentAmmo;
    private float refillTimer;

    public int CurrentAmmo => currentAmmo;
    public int Capacity => capacity;
    public float RefillRemaining => currentAmmo >= capacity ? 0 : Mathf.Max(0, refillInterval - refillTimer);

    private void Awake()
    {
        currentAmmo = capacity;
        refillTimer = 0;
    }

    private void Update()
    {
        if (currentAmmo >= capacity)
            return;

        refillTimer += Time.deltaTime;
        while (refillTimer >= refillInterval && currentAmmo < capacity)
        {
            refillTimer -= refillInterval;
            currentAmmo++;
        }
        if (currentAmmo >= capacity)
            refillTimer = 0;
    }

    public bool TryConsume()
    {
        if (!isActiveAndEnabled || currentAmmo <= 0)
            return false;

        // 首次离开满仓开始计时；连续消耗不重置已进行的补弹。
        if (currentAmmo == capacity)
            refillTimer = 0;
        currentAmmo--;
        return true;
    }

    private void OnValidate()
    {
        capacity = Mathf.Max(1, capacity);
        refillInterval = Mathf.Max(.01f, refillInterval);
        currentAmmo = Mathf.Clamp(currentAmmo, 0, capacity);
    }
}
