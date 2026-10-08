using UnityEngine;

// 地面非战斗状态的公共父类：IdleState 与 MoveState 共用“发现玩家进入战斗”的逻辑。
// 联动链：Entity.Update -> 当前 Idle/Move.Update -> 本类.Update -> Enemy.PlayerDetection。
public class Enemy_GroundState : EnemyState
{
    // 【依赖传递】继续交给 EnemyState 绑定敌人组件，交给 EntityState 保存动画 Bool 名称。
    public Enemy_GroundState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    // 【视野入口】先递减公共计时器、同步动画，再用 Enemy.cs 的前向射线判断玩家是否可见。
    public override void Update()
    {
        base.Update();
        if (enemy.PlayerDetection() == true)
        {
            // Battle.Enter 会从同一检测方法取得玩家 Transform，并检查是否需要立即后退。
            stateMachine.ChangeState(enemy.BattleState);
        }
        // 注意：ChangeState 不会结束调用栈；返回 Idle/Move.Update 后，其剩余逻辑仍会执行。
    }
}
