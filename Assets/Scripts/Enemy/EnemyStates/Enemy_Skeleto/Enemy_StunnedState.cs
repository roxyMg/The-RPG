using UnityEngine;

public class Enemy_StunnedState : EnemyState
{
    private Enemy_VFX vFX;
    public Enemy_StunnedState(Enemy enemy,StateMachine stateMachine, string animBoolName):base(enemy,stateMachine,animBoolName)
    {
        vFX = enemy.GetComponent<Enemy_VFX>();
    }
    public override void Enter()
    {
        base.Enter();

        vFX.EnableAttackAlert(false);
        enemy.EnableCounterWindow(false);
        
        stateTimer = enemy.stunnedDuration;
        // 命中已启动冲击时保留其速度；独立触发失衡则使用冲击 1。
        if (!enemy.isKnocked)
            enemy.GetComponent<Entity_HitReaction>()?.ReceiveImpact(1, -enemy.facingDirection);
    }

    public override void Update()
    {
        base.Update();
        if(stateTimer <= 0)
           stateMachine.ChangeState(enemy.IdleState);
    }

}
