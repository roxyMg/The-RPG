using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// 玩家实体：创建全部状态、持有移动参数，并把新输入系统的数据暴露给状态对象。
[RequireComponent(typeof(Player_PropulsionAmmo), typeof(Entity_Invulnerability), typeof(Player_LethalAmmo))]
[RequireComponent(typeof(Player_Execution))]
public class Player : Entity
{ 
    public static event Action OnPlayerDeath;
    // 所有状态在 Awake 中只创建一次，之后由 StateMachine 重复进入和退出。
    // 【输入配置联动】PlayerInputSet.cs 从 Assets/Settings/PlayerInputSet.inputactions 自动生成，勿手改生成类。
    // 当前绑定：WASD 移动、空格跳跃、左 Shift 冲刺、鼠标左键攻击；重新绑定应编辑 inputactions 资源。
    // 新增 Dash 由 PlayerState 消费，Attack 由地面/空中父状态与 PlayerAttackState 的连击缓存消费。
    public PlayerInputSet input{get; private set;}
    public PlayerIdleState idleState{get; private set;}
    public PlayerMoveState moveState{get; private set;}
    public PlayerJumpState jumpState{get; private set;} 
    public PlayerFallState fallState{get; private set;}
    public PlayerWallSlideState wallSlideState{get; private set;} 
    public PlayerWallJumpState wallJumpState{get; private set;}
    public PlayerDashState dashState{get; private set;}
    public PlayerAttackState attackState{get; private set;}
    public PlayerJumpAttackState jumpAttackState{get; private set;}
    public PlayerDeadState deadState{get; private set;} 
    public PlayerExecutionState executionState { get; private set; }
    public Player_Execution execution { get; private set; }
    public PlayerCounterAttackState counterAttackState{get; private set;}
    
   [Header("Movement details")]
    // 水平移动速度；jumpForce 是起跳瞬间的垂直速度。
    public float moveSpeed;
    public float jumpForce = 5;//跳跃高度
    public Vector2 wallJumpDirection;//跳跃角度

    [Range(0,1)]
    public float inAirMoveMultiplier = .7f;//空中速度控制
    [Range(0,1)]
    public float wallSlideSlowMultiplier = .7f;//华强速度
    [Space]
    // dashDuration 以秒为单位，dashSpeed 为冲刺时的水平速度。
    [Header("枪铳推进 Dash")]
    public float dashDuration = .25f;
    public float dashSpeed = 20;
    [Tooltip("Dash 开始后的无敌秒数，生效时间不会超过本次 Dash。")]
    [Min(0)] public float dashInvulnerabilityDuration = .12f;
    public Player_LethalAmmo lethalAmmo { get; private set; }
    public Player_PropulsionAmmo propulsionAmmo { get; private set; }
    public Entity_Invulnerability invulnerability { get; private set; }
    public Vector2 moveInput{get; private set;}

    protected override void Awake()
    {
        base.Awake();
        execution = GetComponent<Player_Execution>();
        lethalAmmo = GetComponent<Player_LethalAmmo>();
        propulsionAmmo = GetComponent<Player_PropulsionAmmo>();
        invulnerability = GetComponent<Entity_Invulnerability>();
        // 必须先创建 input，因为每个 PlayerState 构造时都会缓存 player.input。
        input = new PlayerInputSet();
        idleState = new PlayerIdleState(this, stateMachine,"Idle");
        moveState = new PlayerMoveState(this, stateMachine,"Move");
        // Jump 与 Fall 共用 JumpFall 动画 Bool，靠 yVelocity 区分上升和下落。
        jumpState = new PlayerJumpState(this, stateMachine,"JumpFall");
        fallState = new PlayerFallState(this, stateMachine,"JumpFall");
        wallSlideState = new PlayerWallSlideState(this, stateMachine,"WallSlide");
        wallJumpState = new PlayerWallJumpState(this, stateMachine,"JumpFall");//墙上跳跃
        dashState = new PlayerDashState(this, stateMachine,"Dash");
        attackState = new PlayerAttackState(this, stateMachine,"Attack");
        jumpAttackState = new PlayerJumpAttackState(this, stateMachine,"JumpAttack");
        deadState = new PlayerDeadState(this, stateMachine,"Dead");
        counterAttackState = new PlayerCounterAttackState(this,stateMachine,"CounterAttack");
        executionState = new PlayerExecutionState(this, stateMachine, "Execution");
    }

    protected override void Start()
    {
        base.Start();
        // StateMachine 在完成全部状态创建后，才以 Idle 作为初始状态进入。
        stateMachine.Initialize(idleState);
    }

    public override void EntityDeath()
    {
        base.EntityDeath();

        OnPlayerDeath?.Invoke();
        stateMachine.ChangeState(deadState);
    }

    public bool TryStartExecution()
    {
        if (!isActiveAndEnabled || !stateMachine.canChangeState || !groundDetected || isKnocked ||
            stateMachine.currentState == deadState || stateMachine.currentState == dashState ||
            stateMachine.currentState == executionState || execution == null || !execution.TryPrepare())
            return false;
        if (queuedAttackCo != null)
        {
            StopCoroutine(queuedAttackCo);
            queuedAttackCo = null;
        }
        stateMachine.ChangeState(executionState);
        return true;
    }

    public bool TryStartCounterAttack()
    {
        if (!isActiveAndEnabled || !stateMachine.canChangeState || isKnocked ||
            stateMachine.currentState == deadState || stateMachine.currentState == counterAttackState ||
            stateMachine.currentState == executionState)
            return false;
        if (lethalAmmo == null || !lethalAmmo.TryConsume())
            return false;
        if (queuedAttackCo != null)
        {
            StopCoroutine(queuedAttackCo);
            queuedAttackCo = null;
        }
        stateMachine.ChangeState(counterAttackState);
        return true;
    }

    // 普通冲刺和墙跳取消共用入口，检查通过后只消耗一发。
    public bool TryStartDash(bool allowWhileTouchingWall = false)
    {
        if (!isActiveAndEnabled || !stateMachine.canChangeState ||
            stateMachine.currentState == dashState || stateMachine.currentState == deadState ||
            stateMachine.currentState == executionState ||
            isKnocked || (!allowWhileTouchingWall && wallDetected))
            return false;

        if (!propulsionAmmo.TryConsume())
            return false;

        // 防止已排队的连击在帧末把 Dash 覆盖回 Attack。
        if (queuedAttackCo != null)
        {
            StopCoroutine(queuedAttackCo);
            queuedAttackCo = null;
        }
        stateMachine.ChangeState(dashState);
        return true;
    }

    // 连击时把重新进入 Attack 延迟到帧末，让 Animator 有一帧看到 Attack=false。
    public void EnterAttackStateWithDelay(Attack_MoveDefinition nextMove = null)
    {
        // 注意：当前 if 没有大括号，只有 StopCoroutine 受条件控制；启动新协程每次都会执行。
        if(queuedAttackCo != null)
           StopCoroutine(queuedAttackCo);
           queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo(nextMove));
    }
    private IEnumerator EnterAttackStateWithDelayCo(Attack_MoveDefinition nextMove)//协程
    {
        // 等到本帧结束后再次进入攻击状态，以触发下一段连击动画。
        yield return new WaitForEndOfFrame();
        queuedAttackCo = null;
        // 帧末仅允许仍在攻击中的派生请求继续，避免旧请求覆盖 Dash 或处决。
        if (!isActiveAndEnabled || !stateMachine.canChangeState || stateMachine.currentState != attackState)
            yield break;
        attackState.PrepareNextMove(nextMove);
        stateMachine.ChangeState(attackState);
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += HandleMovementPerformed;
        input.Player.Movement.canceled += HandleMovementCanceled;
    }

    private void OnDisable()
    {
        input.Player.Movement.performed -= HandleMovementPerformed;
        input.Player.Movement.canceled -= HandleMovementCanceled;
        moveInput = Vector2.zero;
        input.Disable();
        execution?.EndExecution();
    }

    private void HandleMovementPerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void HandleMovementCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

}
