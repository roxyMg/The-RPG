using UnityEngine;

// 空中攻击状态：落地时触发第二阶段动画，并在动画事件完成后回到 Idle。
public class PlayerJumpAttackState : PlayerState
{
    // 防止落地阶段在连续多个物理帧中重复触发。
    private bool touchedGround;
    public PlayerJumpAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 每次进入空中攻击都允许重新检测一次落地。
        touchedGround = false;
    }
    public override void Update()
    {
        base.Update();
        // 公共更新已切走时，不再执行旧状态逻辑。
        if (stateMachine.currentState != this)
            return;

        if(player.groundDetected && !touchedGround)
        {
            touchedGround = true;
            // Animator 必须包含 JumpAttack_Trigger；它通常启动落地攻击动画。
            anim.SetTrigger("JumpAttack_Trigger");//和动画器一致
            player.SetVelocity(0, rb.velocity.y);
        }

        // 只有“动画事件已到达”并且“角色已落地”两个条件同时满足才退出。
        if(triggerCalled && player.groundDetected)
           stateMachine.ChangeState(player.idleState);
    }
}
