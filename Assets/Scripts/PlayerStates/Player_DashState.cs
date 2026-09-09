using UnityEngine;

// 冲刺状态：暂时关闭重力、锁定进入瞬间的方向，并在结束时恢复刚体设置。
public class PlayerDashState : PlayerState
{
    // 保存进入冲刺前的重力，避免把 Inspector 中的自定义值永久覆盖。
    private float originalGravityScale;
    // 冲刺方向在 Enter 时确定，冲刺期间不随新的移动输入改变。
    private float dashDirection;
    public PlayerDashState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // EntityState.Update 会让该计时器按秒递减。
        stateTimer = player.dashDuration;       

        originalGravityScale = rb.gravityScale;//dash状态重力
        rb.gravityScale = 0;

        dashDirection = player.moveInput.x;

        if (dashDirection == 0)
            dashDirection = player.facingDirection;//dash方向根据x输入变化。如果没有则面向方向
    }

    public override void Update()
    {
        base.Update();
        // CancelDash 可能切换状态，但当前方法不会立刻 return，后续代码本帧仍会继续执行。
        CancelDash();
 
        // 冲刺期间每帧固定水平速度，并把垂直速度固定为 4。
        player.SetVelocity(player.dashSpeed * dashDirection, 4);

        if(stateTimer < 0)
        {
            if(player.groundDetected)
               stateMachine.ChangeState(player.idleState);
            else
               stateMachine.ChangeState(player.fallState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        // 无论因计时结束还是撞墙退出，都会停止速度并恢复原重力。
        player.SetVelocity(0, 0);
        rb.gravityScale = originalGravityScale;
    }

    private void CancelDash()//取消冲刺如果需要的话
    {
        // 撞墙时：脚下有地面回 Idle，否则进入 WallSlide。
        if(player.wallDetected)
        {
            if(player.groundDetected)
               stateMachine.ChangeState(player.idleState);
            else
               stateMachine.ChangeState(player.wallSlideState);
        }
    }

}
