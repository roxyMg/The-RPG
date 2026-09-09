using UnityEngine;

// 所有角色状态的共同生命周期，并负责维护对应的 Animator 布尔参数。
public abstract class EntityState
{
    protected StateMachine stateMachine;
    // 必须与 Animator Controller 中的 Bool 参数名称一致。
    protected string animBoolName;
    // anim 和 rb 由 PlayerState / EnemyState 构造函数从具体实体中取得。
    protected Animator anim;
    protected Rigidbody2D rb;
    protected float stateTimer;//计时器
    // 动画片段末尾通过 Entity_AnimationTriggers 回调后置为 true。
    protected bool triggerCalled;//触发器（看攻击那儿）\

    public EntityState(StateMachine stateMachine, string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

     // 每次进入状态时开启动画参数，并清除上一次进入留下的动画事件标记。
     public virtual void Enter()
  {
    anim.SetBool(animBoolName, true);
    // Code to execute when entering the state
    triggerCalled = false;
  }

  // 所有状态计时器统一按“剩余秒数”递减；需要计时的状态在 Enter 中赋初值。
  public virtual void Update()
  {
   stateTimer -= Time.deltaTime; 

  }

  // 离开状态时关闭同名动画参数，避免 Animator 保持旧状态条件。
  public virtual void Exit()
{
    anim.SetBool(animBoolName, false);
    // Code to execute when exiting the state
  }

  // Animation Event 的统一入口；状态在自己的 Update 中决定收到事件后如何退出。
  public void CallAnimationTrigger()
  {
    triggerCalled = true;
  }
}
