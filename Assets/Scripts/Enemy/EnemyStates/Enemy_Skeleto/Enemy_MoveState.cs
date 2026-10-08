using UnityEngine;

// 敌人巡逻状态：沿当前朝向移动，遇墙或前方无地面时掉头。
// 继承 Enemy_GroundState.cs：巡逻时检测到可见玩家就切入 Enemy_BattleState。
public class Enemy_MoveState : Enemy_GroundState
{
    // 【构造】Enemy_Skeleton.Awake 传入共享组件和 Animator 的 Move Bool。
    public Enemy_MoveState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {   
    }
    // 【进入巡逻】只执行 EntityState.Enter 的动画开关与事件重置，无额外计时或速度初始化。
    public override void Enter()
    {
        base.Enter();
    }
    // 【巡逻更新】父类先处理玩家视野，再应用巡逻速度和地面/墙面掉头判断。
    // 父类切入 Battle 后不会中断本方法，本帧仍可能设置巡逻速度或翻面。
    public override void Update()
    {
        base.Update();
        // Enemy.cs 提供 moveSpeed；Entity.SetVelocity 同时设置刚体速度并自动校正面朝方向。
        // 先按本帧朝向设置速度；如果随后翻面，新方向的速度会在下一帧生效。
        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDirection, rb.linearVelocity.y);
        // Entity.cs 每帧先刷新射线结果；检测点放置位置决定 groundDetected 代表脚下还是前方。
        // 无地面或两条墙体射线同时命中时，通过 Entity.Flip 翻转视觉与 facingDirection。
        if (!enemy.groundDetected || enemy.wallDetected)
        {
            enemy.Flip();
        }
    }
}

