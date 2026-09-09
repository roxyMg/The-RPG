using UnityEngine;

// 地面移动状态：根据水平输入设置速度，停止输入或遇墙时回到 Idle。
public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }
    public override void Update()
    {
        base.Update();

        // 注意：当前 if 没有大括号，只有 ChangeState 受条件控制。
        if (player.moveInput.x == 0 || player.wallDetected)
            stateMachine.ChangeState(player.idleState); 
            // SetVelocity 无论条件是否成立都会在本帧执行。
            player.SetVelocity(player.moveInput.x * player.moveSpeed, rb.linearVelocity.y);
    }

}
