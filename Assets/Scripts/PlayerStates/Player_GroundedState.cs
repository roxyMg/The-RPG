using UnityEngine;

// 所有地面状态的公共父类：统一处理离地、跳跃和地面攻击的状态切换。
public class PlayerGroundedState : PlayerState
{
public PlayerGroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {      
    }
public override void Update()
    {
        base.Update();
        // 只有正在下落且地面射线未命中时才进入 Fall，避免起跳帧被这里误判。
        if(rb.linearVelocity.y < 0 && player.groundDetected == false)
           stateMachine.ChangeState(player.fallState);
        if(input.Player.Jump.WasPerformedThisFrame())
           stateMachine.ChangeState(player.jumpState);
        if(input.Player.Attack.WasPerformedThisFrame())
           stateMachine.ChangeState(player.attackState);
        // 三个判断彼此独立；同一帧同时满足时，靠后的状态切换会覆盖靠前的切换。
    }









}
