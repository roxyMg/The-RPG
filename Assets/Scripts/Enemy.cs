using UnityEngine;

// 敌人公共数据容器；具体敌人子类负责创建并初始化这些状态。
public class Enemy : Entity
{
    // 当前骨骼敌人只启用了待机和移动状态。
    public Enemy_IdleState IdleState;
    public Enemy_MoveState MoveState;

    [Header("Enemy Movement details")]
    // idleTime 以秒为单位，结束后切换到巡逻移动。
    public float idleTime = 2;
    public float moveSpeed = 1.4f;
    [Range(0,2)]
    // 每帧同步到 Animator 的同名 Float 参数，用于整体调节敌人动画播放速度。
    public float AnimSpeedMultiplier = 1;

}
