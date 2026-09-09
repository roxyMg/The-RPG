using UnityEngine;

// 敌人巡逻状态：沿当前朝向移动，遇墙或前方无地面时掉头。
public class Enemy_MoveState : EnemyState
{
    public Enemy_MoveState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Update()
    {
        base.Update();
        // 先按本帧朝向设置速度；如果随后翻面，新方向的速度会在下一帧生效。
        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDirection, rb.linearVelocity.y);
        // groundDetected 为假表示前方/脚下无地面，wallDetected 为真表示前方有完整墙面。
        if (!enemy.groundDetected || enemy.wallDetected)
        {
            enemy.Flip();
        }
    }
}

