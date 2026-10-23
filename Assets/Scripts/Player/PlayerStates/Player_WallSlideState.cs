using UnityEngine;

// 贴墙下滑状态：限制垂直速度，并处理墙跳、离墙和落地。
public class PlayerWallSlideState : PlayerState
{
    public PlayerWallSlideState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }
    public override void Update()
    {
        base.Update();
        // 公共更新已切走时，不再执行旧状态逻辑。
        if (stateMachine.currentState != this)
            return;
        // 先应用本帧贴墙速度，再判断是否需要离开该状态。
        HandleWallSlide();//切记，一定要调用

        if(input.Player.Jump.WasPressedThisFrame())
           stateMachine.ChangeState(player.wallJumpState);

        if(player.wallDetected == false)
           stateMachine.ChangeState(player.fallState);

        if(player.groundDetected)
        {
           // 落地后主动翻面，使角色不再继续朝向刚才贴住的墙。
           stateMachine.ChangeState(player.idleState);
           player.Flip();
        }
    }
    private void HandleWallSlide()
    {
        // 向下输入时不衰减 y 速度；否则每帧按倍率缩小当前下落速度。
        // x 速度这里直接使用输入值，没有乘 moveSpeed。
        if(player.moveInput.y < 0)
          player.SetVelocity(player.moveInput.x, rb.velocity.y);
        else
          player.SetVelocity(player.moveInput.x, rb.velocity.y* player.wallSlideSlowMultiplier);  
    }
}
