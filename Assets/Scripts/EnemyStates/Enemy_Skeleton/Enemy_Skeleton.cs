using UnityEngine;

// 骷髅敌人的实体入口：组装该敌人可用的状态并指定初始状态。
public class Enemy_Skeleton : Enemy
{
   protected override void Awake()
    {
        base.Awake();
        // 状态构造时会从当前 Enemy 缓存 Animator 和 Rigidbody2D。
        IdleState = new Enemy_IdleState(this, stateMachine, "Idle");
        MoveState = new Enemy_MoveState(this, stateMachine, "Move");
        // 攻击状态尚未接入；启用前还需要在 Enemy 中提供对应引用和切换条件。
        //AttackState = new Enemy_AttackState(this, stateMachine, "Attack");
    }
   protected override void Start()
    {
        base.Start();
        // 骷髅生成后先进入待机，再由待机计时切到移动。
        stateMachine.Initialize(IdleState);
    }
}
