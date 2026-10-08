using UnityEngine;

// 骷髅敌人的实体入口：组装该敌人可用的状态并指定初始状态。
// 挂在骷髅根对象上；Entity.cs 提供刚体与子 Animator，Enemy.cs 提供 Inspector 配置。
public class Enemy_Skeleton : Enemy , ICounterable
{
    public bool CanBeCountered { get => canBeStunned;}

    // 【组装状态】先调用 Entity.Awake 获取组件和创建状态机，再构造四个可复用状态对象。
    protected override void Awake()
    {
        base.Awake();
        // 每个状态共享当前实体与状态机；字符串对应 Enemy_Skeleton.controller 的 Bool 参数。
        // Idle/Move 经 Enemy_GroundState 共用玩家视野检测；Battle/Attack 使用独立战斗逻辑。
        IdleState = new Enemy_IdleState(this, stateMachine, "Idle");
        MoveState = new Enemy_MoveState(this, stateMachine, "Move");
        AttackState = new Enemy_AttackState(this, stateMachine, "Attack");
        BattleState = new Enemy_BattleState(this, stateMachine, "Battle");
        DeadState = new Enemy_DeadState(this, stateMachine, "Idle");
        StunnedState = new Enemy_StunnedState(this, stateMachine, "Stunned");
    }
   // 【启动状态机】状态构造完成后，在 Start 正式进入 Idle 并启用 Idle 动画 Bool。
   protected override void Start()
    {
        base.Start();
        // 骷髅生成后先进入待机，再由待机计时切到移动。
        stateMachine.Initialize(IdleState);
    }

    [ContextMenu("眩晕敌人")]
    public void HandleCounter()
    {
        if(CanBeCountered == false)
           return;
           
        stateMachine.ChangeState(StunnedState);
    }
}
