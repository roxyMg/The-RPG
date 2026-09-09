using UnityEngine;

// 跳跃与下落的公共父类：提供空中水平控制和空中攻击入口。
public class PlayerAiredState : PlayerState
{
    public PlayerAiredState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        
    }
   
    public override void Update()
    {
       base.Update();
       // 有水平输入时才覆盖 x 速度；无输入时保留进入空中前的水平惯性。
       if(player.moveInput.x != 0)
          player.SetVelocity(player.moveInput.x * (player.moveSpeed * player.inAirMoveMultiplier), rb.linearVelocity.y);   
       
       // 空中攻击由 Jump 和 Fall 共用，进入后由落地与动画事件共同决定退出。
       if(input.Player.Attack.WasPressedThisFrame())
          stateMachine.ChangeState(player.jumpAttackState);
    
    }
}
