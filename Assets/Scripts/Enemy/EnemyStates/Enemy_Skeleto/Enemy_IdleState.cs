using UnityEngine;

// 敌人待机状态：进入时启动倒计时，结束后开始巡逻。
// 继承 Enemy_GroundState.cs，因此等待期间也能发现玩家并切入 Battle。
public class Enemy_IdleState : Enemy_GroundState
{
    // 【构造】由 Enemy_Skeleton.Awake 创建，使用 Animator 的 Idle Bool。
    public Enemy_IdleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {    
    }
    // 【进入待机】EntityState.Enter 开启动画并清除事件标记，再重置等待时间。
    // 当前这里没有清零刚体速度；从 Battle/Attack 返回时，不会仅因进入 Idle 而自动停止移动。
    public override void Enter()
    {
        base.Enter();
        // stateTimer 会在 EntityState.Update 中每帧递减。
        stateTimer = enemy.idleTime;
    }
    // 【等待与切换】父类先进行计时、动画和玩家检测，再检查是否开始巡逻。
    public override void Update()
    {
        base.Update();
        // 倒计时到零后进入 Move；若父类本帧刚切入 Battle，此判断仍会执行并可能覆盖该切换。
        if (stateTimer <= 0)
        stateMachine.ChangeState(enemy.MoveState);
    }
}
