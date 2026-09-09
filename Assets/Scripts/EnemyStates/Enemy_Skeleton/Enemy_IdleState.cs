using UnityEngine;

// 敌人待机状态：进入时启动倒计时，结束后开始巡逻。
public class Enemy_IdleState : EnemyState
{
    public Enemy_IdleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        
    }
    public override void Enter()
    {
        base.Enter();
        // stateTimer 会在 EntityState.Update 中每帧递减。
        stateTimer = enemy.idleTime;
    }
    public override void Update()
    {
        base.Update();
        // 倒计时到零后进入移动状态。
        if (stateTimer <= 0)
        stateMachine.ChangeState(enemy.MoveState);
    }
}
