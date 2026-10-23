using System;
using System.Collections;
using UnityEngine;

// 玩家和敌人的公共实体基类：集中管理组件、状态机、朝向与环境检测。
public class Entity : MonoBehaviour
{
    public event Action OnFlipped;
    // 状态对象通过这两个只读属性共享 Unity 组件。
    public Animator anim{get; private set;}
    public Rigidbody2D rb{get; private set;}
    protected StateMachine stateMachine;

    [Header("攻击属性")]
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
    
    [Header("检测地面和墙体")]
    // 【共享检测层】改为 protected 后，Enemy.PlayerDetection 可以把地形层与玩家层合并查询。
    // 这样敌人视线和公共地面/墙面检测使用同一套地形层配置；仍通过 Inspector 序列化赋值。
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private Transform groundCheck;//地面检测
    [SerializeField] private Transform primaryWallCheck;//初级墙体检测
    [SerializeField] private Transform secondaryWallCheck;//二级墙体检测
    // 检测结果每帧先刷新，再交给当前状态读取。
    public bool groundDetected{get; private set;}//这俩都是检测是否在地面或者墙上
    public bool wallDetected{get; private set;}

    private Coroutine knockbackCo;
    public bool isKnocked;

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
    // 【子类扩展入口】开放为 protected virtual，让 Enemy.Update 可以重写并通过 base.Update 复用调度。
    // 子类需保留基类调用，否则环境检测和 StateMachine 当前状态更新都会被跳过。
    protected virtual void Update()
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

    public virtual void EntityDeath()
    {
        // 死亡接管行为前，终止普通击退和排队连击。
        CancelKnockback();

        if (queuedAttackCo != null)
        {
            StopCoroutine(queuedAttackCo);
            queuedAttackCo = null;
        }
    }

    public void CancelKnockback()
    {
        if (knockbackCo != null)
        {
            StopCoroutine(knockbackCo);
            knockbackCo = null;
        }

        // 不清零速度，让死亡状态接管刚体。
        isKnocked = false;
    }
    public void ReceiveKnockback(Vector2 knockback, float duration)
    {
        if(knockbackCo != null)
           StopCoroutine(knockbackCo);

        knockbackCo = StartCoroutine(KnockbackCo(knockback,duration));   
    }

    private IEnumerator KnockbackCo(Vector2 knockback, float duration)
    {
        isKnocked = true;
        rb.velocity = knockback;

        yield return new WaitForSeconds(duration);

        rb.velocity = Vector2.zero;
        isKnocked = false;
        knockbackCo = null;
    }


    // 直接设置刚体速度；非零水平速度还会自动校正角色朝向。
    public void SetVelocity(float xVelocity, float yVelocity)//设置速度
    {
        if(isKnocked)
           return;

        rb.velocity = new Vector2(xVelocity, yVelocity);
        HandFlip(xVelocity);
    }

    // xVelocity 为 0 时保持原朝向，只有移动方向与当前朝向相反时才翻转。
    // 【独立朝向入口】开放为 public，供 Enemy_BattleState 后退时仅转向、不覆盖已设置的反向速度。
    // SetVelocity 会自动调用本方法；需要“移动方向与面朝方向不同”时才分别设置速度和朝向。
    public void HandFlip(float xVelocity)//角色如果向某方向移动，判断是否需要转向
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

        OnFlipped?.Invoke();
    }

    // 地面使用一条向下射线；墙面要求上、下两条水平射线同时命中，避免把矮障碍当成完整墙面。
    private void HandleCollisionDetection()//物体检测
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround) 
                    && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }   

    // 仅在 Scene 视图绘制检测范围，便于调整三个检测点和射线距离。
    // 【子类绘制入口】Enemy.OnDrawGizmos 重写后先调用本方法，再追加玩家视野与战斗距离参考线。
    protected virtual void OnDrawGizmos()//射出一条射线检测
    {
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance));//检测地面
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDirection, 0));//检测墙壁
        Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * facingDirection, 0));
    }
}
