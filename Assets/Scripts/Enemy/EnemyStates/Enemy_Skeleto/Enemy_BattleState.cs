using UnityEngine;

// 战斗状态：由 Idle/Move 的 Enemy_GroundState 玩家检测进入，负责追击、脱战和切入攻击。
// 继承 EnemyState 而非 GroundState，避免战斗期间被公共玩家检测反复重新进入 Battle。
public class Enemy_BattleState : EnemyState
{
    // 【战斗记忆】缓存首次检测得到的玩家 Transform；离开状态后仍保留，重新进入不会自动换目标。
    private Transform player;
    // 最后看见玩家的绝对游戏时间；与 Enemy.cs 的同名公开字段互不关联。
    // 当前 Enter 不重置此值，时间戳只在 UpdateBattleTimer 中更新。
    private float lastTimeWasInBattle;
    // 【构造】Enemy_Skeleton.Awake 提供共享状态机、敌人组件和 Animator 的 Battle Bool。
    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        
    }
    // 【进入战斗】EntityState.Enter 开启动画，然后获取目标并处理一次过近时的后退。
    public override void Enter()
    {
        base.Enter();

        UpdateBattleTimer();

        // 依赖进入前 Enemy.PlayerDetection 已命中玩家；目标已缓存时不重新查询 Transform。
        if (player == null)
        player = enemy.GetPlayerReference();

        // 后退只在进入时执行，距离阈值和速度来自 Enemy.cs，并没有单独的后退持续计时。
        if (ShouldRetreat())
        {
            // 直接写 rb 而非 Entity.SetVelocity，避免后退移动方向把角色自动翻成背对玩家。
            rb.velocity = new Vector2(enemy.retreatVelocity.x * -DirectionToPlayer(), enemy.retreatVelocity.y);
            // 仍通过 Entity.HandFlip 保持面朝玩家，形成“面向目标后退”的表现。
            enemy.HandFlip(DirectionToPlayer());
        }
    }
    // 【每帧战斗】先同步动画 -> 可见时续期 -> 检查脱战 -> 决定攻击或追击。
    public override void Update()
    {
        base.Update();
        // 视野来自 Enemy.cs 的前向且可被地形遮挡的射线；看见玩家时刷新最后可见时刻。
        if (enemy.PlayerDetection())
            UpdateBattleTimer();

        // 丢失视野超过 battleTimeDuration 后回 Idle，但当前这里没有 return。
        // 因此本帧后面的攻击/追击代码仍会执行，可能再次切状态或写入速度。
        if (BattleTimeIsOver())
            stateMachine.ChangeState(enemy.IdleState);

        // 攻击要求“水平距离在范围内”且“射线仍看到玩家”；随后由 AttackState 等待动画事件结束。
        if (WithInAttackRange() && enemy.PlayerDetection() == true)
            stateMachine.ChangeState(enemy.AttackState);
        else
            // 即使暂时看不到玩家，只要走到这个分支仍会按缓存目标追击，直到脱战逻辑生效。
            // Entity.SetVelocity 自动转向玩家；这里没有巡逻状态那样的墙面/悬崖掉头判断。
            enemy.SetVelocity(enemy.battleMoveSpeed * DirectionToPlayer(), rb.velocity.y);
        
    }

    // 【脱战计时工具】保存绝对时刻，不使用 EntityState 的 stateTimer 倒计时。
    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;

    private bool BattleTimeIsOver() => Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;

    // 【范围工具】攻击和后退都复用目标 Transform 间水平距离；严格小于阈值才满足，不计算垂直距离。
    private bool WithInAttackRange() => DistanceToPlayer() < enemy.AttackRange;

    private bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;
    // 【目标距离】目标为空时返回极大值，让攻击/后退范围判断自然失败。
    private float DistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;
        
        return Mathf.Abs(player.position.x - enemy.transform.position.x);
    }
    // 【目标方向】右侧返回 1，否则返回 -1；目标为空返回 0，追击水平速度随之为 0。
    // 用于朝向修正、追击速度和后退的反方向速度。
    private int DirectionToPlayer()
    {
        if (player == null)
            return 0;
        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }

}
