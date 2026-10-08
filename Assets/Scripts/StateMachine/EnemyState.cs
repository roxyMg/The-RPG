using UnityEngine;

// 敌人状态公共基类：继承 EntityState.cs 的 Enter/Update/Exit、倒计时和动画事件标记。
// 继承链：Idle/Move -> Enemy_GroundState -> EnemyState；Battle/Attack -> EnemyState。
public class EnemyState : EntityState
{
    // 【共享实体】具体状态从此引用读取 Enemy.cs 参数，调用 Entity.cs 的速度、翻面等方法。
    protected Enemy enemy;

    // 【依赖绑定】Enemy_Skeleton.cs 创建状态时传入同一个敌人、状态机和 Animator Bool 名称。
    // Entity.Awake 必须先完成组件初始化；状态不是 MonoBehaviour，无法自行 GetComponent。
    public EnemyState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.enemy = enemy;
        anim = enemy.anim;
        rb = enemy.rb;
    }
    // 【公共动画同步】EntityState.Update 调用虚方法，所有敌人状态都自动执行这一段。
    // 与 Assets/Animations/Animator_Controllers/Enemy_Skeleton.controller 的 Float 参数联动。
    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
        // 保留父类扩展入口；当前 EntityState.UpdateAnimationParameters 没有额外实现。

        // 战斗动画速度相对普通巡逻速度计算，不使用 MoveAnimSpeedMultiplier 再次相乘。
        float BattleAnimSpeedMultiplier = enemy.battleMoveSpeed / enemy.moveSpeed;

        // 三个参数名称需与 Controller 完全一致；xVelocity 是带正负方向的刚体水平速度。
        anim.SetFloat("BattleAnimSpeedMultiplier", BattleAnimSpeedMultiplier);
        anim.SetFloat("MoveAnimSpeedMultiplier", enemy.MoveAnimSpeedMultiplier); // 普通移动动画倍率来自 Enemy.cs 的 Inspector 配置。
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
    }

}
