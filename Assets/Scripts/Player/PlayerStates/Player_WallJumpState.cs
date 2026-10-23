using UnityEngine;

// 墙跳状态：把角色推向墙的反方向，并在短暂锁定后恢复常规空中切换。
public class PlayerWallJumpState : PlayerState
{
    // 当前字段只在状态对象创建时初始化，Enter 中没有重置；因此锁定倒计时只对首次墙跳保持初值。
    private float wallJumpLockTimer = 0.15f;//墙跳一个很短的锁定时间

    public PlayerWallJumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 水平分量乘以朝向反值，确保速度指向远离当前墙面的一侧。
        player.SetVelocity(player.wallJumpDirection.x * -player.facingDirection, player.wallJumpDirection.y);
    }

    public override void Update()
    {
        base.Update();

        // 倒计时结束且再次检测到墙面时，优先回到贴墙下滑。
        wallJumpLockTimer -= Time.deltaTime;
        if (wallJumpLockTimer <= 0 && player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
            return;
        } //墙跳出去前 0.15s：暂时禁止重新进入 WallSlide。
        if (TryCancelWallJump())
            return;
        
        // 先处理进入下落，再检查贴墙；两个判断独立，贴墙结果可能覆盖 Fall。
        if(rb.velocity.y < 0)
           stateMachine.ChangeState(player.fallState);

        if(player.wallDetected)
           stateMachine.ChangeState(player.wallSlideState);  
    }
    private bool TryCancelWallJump()//尝试取消墙跳。进行dash
    {
    // 这里不经过 PlayerState.CanDash，可让墙跳状态用冲刺主动取消。
    if (input.Player.Dash.WasPressedThisFrame())
       {
        stateMachine.ChangeState(player.dashState);
        return true;
       }

    return false;
    }
}
