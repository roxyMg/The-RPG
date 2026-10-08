using UnityEngine;

// 地面待机状态：停止水平运动，等待移动输入或父类处理跳跃/攻击。
public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        // 进入待机只清零水平速度，保留当前垂直速度。
        player.SetVelocity(0, rb.linearVelocity.y);
    }
    public override void Update()
    {
        base.Update();

        // 面朝墙并继续向墙输入时保持 Idle，防止 Idle/Move 在贴墙时来回切换。
        if (player.moveInput.x == player.facingDirection && player.wallDetected)//贴到墙上切换为idle状态
            return;
        
        // 这里比较完整 Vector2，因此只有纵向输入时也会进入 Move。
        if (player.moveInput != Vector2.zero)
            stateMachine.ChangeState(player.moveState);
        
    }
}
