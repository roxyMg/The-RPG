using UnityEngine;

// 敌人公共实体：继承 Entity.cs 的组件、状态机、朝向和地面/墙面检测。
// 联动入口：Enemy_Skeleton.cs 创建状态；各 Enemy_*State 读取这里的参数并调用检测方法。
public class Enemy : Entity
{
    // 【状态引用】由 Enemy_Skeleton.Awake 创建，供 StateMachine.ChangeState 切换使用。
    // 通常流程：Idle -> Move；Idle/Move 检测到玩家 -> Battle；Battle 进入攻击距离 -> Attack。
    // Attack 收到动画结束事件 -> Idle；Battle 丢失视野超过持续时间 -> Idle。
    public Enemy_IdleState IdleState;
    public Enemy_MoveState MoveState;
    public Enemy_AttackState AttackState;
    public Enemy_BattleState BattleState;
    public Enemy_DeadState DeadState;
    public Enemy_StunnedState StunnedState;
    // 【战斗配置】供 Enemy_BattleState.cs 的追击、攻击范围、脱战和进入时后退逻辑读取。
    [Header("战斗属性")]
    // 战斗水平追击速度；EnemyState.cs 还用它 / moveSpeed 计算战斗动画速度倍率。
    public float battleMoveSpeed = 3;
    // 攻击判定使用缓存的玩家 Transform 与敌人 Transform 的水平距离，不是射线命中点距离。
    public float AttackRange = 2;
    // 最后一次看见玩家后仍保持战斗的秒数，实际时间戳保存在 Enemy_BattleState 内部。
    public float battleTimeDuration = 5;
    // 仅在进入 Battle 时检查：玩家水平距离小于此值就施加一次后退速度。
    public float minRetreatDistance = 1;
    //当玩家距离小于该值时，敌人会后退// 后退速度的 x 为远离玩家的水平分量，y 为垂直分量；不是位移或持续时间。
    public Vector2 retreatVelocity;//见上


    // 【时间观察字段】当前敌人状态没有使用这里的 lastTimeWasInBattle。
    // Enemy_BattleState.cs 中存在同名私有字段，脱战判断使用的是那一个，二者不是同一份数据。
    public float lastTimeWasInBattle;
    public float inGameTime;
    [Header("眩晕状态")]
    public float stunnedDuration = 1;
    public Vector2 stunnedVelocity = new Vector2(7,7);
    protected bool canBeStunned;

    // 【巡逻配置】IdleState 使用等待时间，MoveState 使用移动速度，EnemyState 同步动画倍率。
    [Header("敌人属性")]
    // idleTime 以秒为单位，结束后切换到巡逻移动。
    public float idleTime = 2;
    // 普通巡逻速度；还作为战斗动画倍率的分母，配置时需避免为 0。
    public float moveSpeed = 1.4f;
    [Range(0,2)]
    // EnemyState.UpdateAnimationParameters 同步到 Animator 的 MoveAnimSpeedMultiplier 参数。
    public float MoveAnimSpeedMultiplier = 1;

    // 【视野配置】从 playerCheck 沿 facingDirection 发射射线；LayerMask 在 Inspector 中配置。
    [Header("检测玩家范围")]
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private Transform playerCheck;
    [SerializeField] private float playerCheckDistance = 10;

    public Transform player {get; private set;}
    [Header("死亡属性")]
    public float deathLaunchForce = 15f;
    public float deathFadeDuration = .4f;
    public float deathHorizontalMultiplier = 1.5f;

    // 【每帧调度】先由 Entity.Update 刷新环境检测、更新当前状态，再记录观察用游戏时间。
    // 状态逻辑读取 Time.time，不依赖 inGameTime；此字段赋值发生在状态更新之后。
    protected override void Update()
    {
        base.Update();
        inGameTime = Time.time;//9.03
    }

    public void EnableCounterWindow(bool enable) => canBeStunned = enable;
    public override void EntityDeath()
    {
        base.EntityDeath();

        stateMachine.ChangeState(DeadState);
    }

    private void HandlePlayerDeath()
    {
        stateMachine.ChangeState(IdleState);
    }


    public void TryEnterBattleState(Transform player)
    {   
        if(stateMachine.currentState == DeadState)
           return;

        if(stateMachine.currentState == BattleState || stateMachine.currentState == AttackState)
           return;

        this.player = player;
        stateMachine.ChangeState(BattleState);
    }

    public Transform GetPlayerReference()
    {
        if(player == null)
           player = PlayerDetection().transform;
        

        return player;
    }

    // 【玩家视野】GroundState 用命中结果进入 Battle；BattleState 用它续期战斗和确认可攻击。
    // 同时检测玩家层和地形层：射线先撞上地形时返回空结果，因此墙面可以遮挡玩家视线。
    public RaycastHit2D PlayerDetection()//检测玩家
    {
        RaycastHit2D hit =
            Physics2D.Raycast(playerCheck.position, Vector2.right * facingDirection, playerCheckDistance, whatIsPlayer | whatIsGround);
        // 不仅要求命中，还要求命中对象自身位于名为 Player 的层；玩家子碰撞体也需正确设层。
        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            return default;
        return hit;
    }
    // 【Scene 调试线】继承 Entity 的地面/墙面线，再显示视野和两种战斗距离的参考线。
    // 红：视野长度；蓝：攻击距离；绿：后退距离。后两者仅辅助观察，实际判定使用目标 Transform 水平距离。
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDirection * playerCheckDistance), playerCheck.position.y));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDirection * AttackRange), playerCheck.position.y));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDirection * minRetreatDistance), playerCheck.position.y));
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += HandlePlayerDeath;
    }

    private void ODisable()
    {
        Player.OnPlayerDeath -= HandlePlayerDeath;
    }

}
