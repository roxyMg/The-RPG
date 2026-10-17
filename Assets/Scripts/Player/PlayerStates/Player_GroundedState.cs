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
        // 公共更新已切走时，不再执行旧状态逻辑。
        if (stateMachine.currentState != this)
            return;
        // 只有正在下落且地面射线未命中时才进入 Fall，避免起跳帧被这里误判。
        if(rb.linearVelocity.y < 0 && player.groundDetected == false)
           stateMachine.ChangeState(player.fallState);
        if(input.Player.Jump.WasPressedThisFrame())
           stateMachine.ChangeState(player.jumpState);
        if(input.Player.Attack.WasPressedThisFrame())
           stateMachine.ChangeState(player.attackState);
        if(stateMachine.currentState == this && input.Player.CounterAttack.WasPressedThisFrame())
           player.TryStartCounterAttack();
        
        //同一帧同时满足时，靠后的状态切换会覆盖靠前的切换。
    }









}
