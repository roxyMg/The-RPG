using UnityEngine;

public class PlayerExecutionState : PlayerState
{
    public PlayerExecutionState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }
    public override void Enter()
    {
        // 处决使用独立 Animator 参数，结束时由基类 Exit 关闭。
        animBoolName = player.execution.AnimationBoolName;
        base.Enter();
        player.execution.BeginExecution();
    }
    public override void Update()
    {
        // 不调用含 Dash 输入的 PlayerState.Update；动作期间只推进处决时序。
        UpdateAnimationParameters();
        if (player.execution.TickExecution(Time.deltaTime))
            stateMachine.ChangeState(player.idleState);
    }
    public override void Exit()
    {
        base.Exit();
        player.execution.EndExecution();
    }
}
