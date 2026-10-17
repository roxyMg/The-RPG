using UnityEngine;

// 下落状态：复用空中控制，并在落地或接触墙面时结束下落。
public class PlayerFallState : PlayerAiredState
{
   public PlayerFallState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Update()
    {
        base.Update();
        // 公共更新已切走时，不再执行旧状态逻辑。
        if (stateMachine.currentState != this)
            return;
        // 这两个判断彼此独立；若同一帧同时检测到地面和墙面，WallSlide 会成为最终状态。
        if(player.groundDetected)
           stateMachine.ChangeState(player.idleState);
        if(player.wallDetected)
           stateMachine.ChangeState(player.wallSlideState);
    }
}
