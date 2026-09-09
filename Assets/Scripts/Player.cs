using System.Collections;
using UnityEngine;

// 玩家实体：创建全部状态、持有移动参数，并把新输入系统的数据暴露给状态对象。
public class Player : Entity
{ 
    // 所有状态在 Awake 中只创建一次，之后由 StateMachine 重复进入和退出。
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
    public float dashDuration = .25f;
    public float dashSpeed = 20;
    public Vector2 moveInput{get; private set;}

    protected override void Awake()
    {
        base.Awake();
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
    }

    protected override void Start()
    {
        base.Start();
        // StateMachine 在完成全部状态创建后，才以 Idle 作为初始状态进入。
        stateMachine.Initialize(idleState);
    }

    // 连击时把重新进入 Attack 延迟到帧末，让 Animator 有一帧看到 Attack=false。
    public void EnterAttackStateWithDelay()
    {
        // 注意：当前 if 没有大括号，只有 StopCoroutine 受条件控制；启动新协程每次都会执行。
        if(queuedAttackCo != null)
           StopCoroutine(queuedAttackCo);
           queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
    }
    private IEnumerator EnterAttackStateWithDelayCo()//协程
    {
        // 等到本帧结束后再次进入攻击状态，以触发下一段连击动画。
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(attackState);
    }

    private void OnEnable()
    {
        input.Enable();
        // performed 持续写入当前二维输入，canceled 在松开时清零。
        // 当前实现每次启用都会追加匿名回调；对象若反复启用，需要留意重复订阅。
        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
    }
    private void OnDisable()
    {
        // 停用输入动作，防止组件禁用后继续接收输入。
        input.Disable();
    }

}
