using UnityEngine;

// 玩家和敌人的公共实体基类：集中管理组件、状态机、朝向与环境检测。
public class Entity : MonoBehaviour
{
    // 状态对象通过这两个只读属性共享 Unity 组件。
    public Animator anim{get; private set;}
    public Rigidbody2D rb{get; private set;}
    protected StateMachine stateMachine;

    [Header("Attack details")]
    // 数组下标与连击段数一一对应，每个 Vector2 表示该段攻击开始时的速度。
    public Vector2[] attackVelocity;
    // 攻击速度保持时间（秒）；超时后攻击状态会把水平速度归零。
    public float attackVelocityDuration = .1f;
    // 两次攻击间隔超过该秒数时，连击从第 0 段重新开始。
    public float comboResetTime = 1;
    protected Coroutine queuedAttackCo;//协程

    private bool facingRight = true;//定义面朝方向 
    // 逻辑朝向只使用 1（右）和 -1（左），用于速度、墙体射线和攻击方向。
    public int facingDirection{get; private set;} = 1 ;//见下面Flip
    
    [Header("Collision detection")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform groundCheck;//地面检测
    [SerializeField] private Transform primaryWallCheck;//初级墙体检测
    [SerializeField] private Transform secondaryWallCheck;//二级墙体检测
    // 检测结果每帧先刷新，再交给当前状态读取。
    public bool groundDetected{get; private set;}//这俩都是检测是否在地面或者墙上
    public bool wallDetected{get; private set;}

    // 子类重写 Awake 时必须调用 base.Awake()，否则组件和状态机不会创建。
    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();
    }
    protected virtual void Start()
    {
        
    }
    private void Update()
    {
        // 顺序很重要：状态逻辑使用的是本帧最新的地面/墙面检测结果。
        HandleCollisionDetection();
        stateMachine.UpdateActiveState();
    }

    // 由动画片段上的 Animation Event 调用，并转发给事件发生时的当前状态。
    public void CallAnimationTrigger()//动画中调用触发器
    {
        stateMachine.currentState.CallAnimationTrigger();
    }

    // 直接设置刚体速度；非零水平速度还会自动校正角色朝向。
    public void SetVelocity(float xVelocity, float yVelocity)//设置速度
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandFlip(xVelocity);
    }

    // xVelocity 为 0 时保持原朝向，只有移动方向与当前朝向相反时才翻转。
    private void HandFlip(float xVelocity)//角色如果向某方向移动，判断是否需要转向
    {
        if (xVelocity > 0 && facingRight == false)
        Flip();
        else if(xVelocity < 0 && facingRight)
        Flip();
        
    }

    // 同时翻转画面朝向和逻辑方向；两者必须始终同步。
    public void Flip()//角色翻转
    {
        transform.Rotate(0,180,0);
        facingRight = !facingRight;
        facingDirection = facingDirection * -1;
       
    }

    // 地面使用一条向下射线；墙面要求上、下两条水平射线同时命中，避免把矮障碍当成完整墙面。
    private void HandleCollisionDetection()//物体检测
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround) 
                    && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }   

    // 仅在 Scene 视图绘制检测范围，便于调整三个检测点和射线距离。
    private void OnDrawGizmos()//射出一条射线检测
    {
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance));//检测地面
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDirection, 0));//检测墙壁
        Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * facingDirection, 0));
    }
}
