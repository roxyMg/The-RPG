using UnityEngine;

// 起跳状态：进入时施加一次向上的速度，越过最高点后交给 Fall。
public class PlayerJumpState : PlayerAiredState
{
   public PlayerJumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
{
}

   public override void Enter()
    {
       base.Enter(); 
       // 保留进入状态前的水平速度，只把垂直速度设置为 jumpForce。
       player.SetVelocity(rb.linearVelocity.x, player.jumpForce);
    }
    public override void Update()
    {
        base.Update();
        // PlayerAiredState 可能已在本帧切到空中攻击；此时不要再用 Fall 覆盖它。
        if (rb.linearVelocity.y < 0 && stateMachine.currentState != player.jumpAttackState)
            stateMachine.ChangeState(player.fallState);
    }


}
