using UnityEngine;

public class PlayerCounterAttackState : PlayerState

{
    private Player_Combat combat;
    private bool counterSomebody;
    public PlayerCounterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat = player.GetComponent<Player_Combat>();
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = combat.GetCounterDuration();//反击窗口
        counterSomebody = combat.CounterAttackPerformed();//进入反击状态检测一次周围有没有可反击目标
        anim.SetBool("CounterAttackperformed", counterSomebody);
        
    }

    public override void Update()
    {
        base.Update();
        // 公共更新已切走时，不再执行旧状态逻辑。
        if (stateMachine.currentState != this)
            return;

        player.SetVelocity(0, rb.linearVelocity.y);        

        if(triggerCalled)
        {
           stateMachine.ChangeState(player.idleState);
           return;
        }

        if(stateTimer<=0 && counterSomebody == false)
           stateMachine.ChangeState(player.idleState);
    }
    public override void Exit()
    {
        base.Exit();
        anim.SetBool("CounterAttackperformed", false);
    }
}
