using UnityEngine;

// 骷髅攻击状态：由 Enemy_BattleState 在距离足够近且玩家仍可见时进入。
// 当前负责攻击动画生命周期和结束后返回 Idle；这里没有伤害判定或主动清零速度的代码。
public class Enemy_AttackState : EnemyState
{
    // 【构造与进入】Enemy_Skeleton 创建该状态，传入 Attack Bool；Enter/Exit 直接继承 EntityState。
    public Enemy_AttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }
    // 【等待动画结束】父类更新计时和动画参数；攻击完成以 Animation Event 为准，不依靠固定秒数。
    // SkeletonAttack.anim 的 CurrentStateTrigger -> Entity_AnimationTriggers.cs -> Entity.cs
    // -> EntityState.CallAnimationTrigger，把当前状态的 triggerCalled 设置为 true。
    public override void Update()
    {
        base.Update();
        // 事件到达后回 Idle；若玩家仍在视野内，Idle 的 GroundState 父类下一帧会再次进入 Battle。
        if (triggerCalled)
        stateMachine.ChangeState(enemy.IdleState);
    }
}
