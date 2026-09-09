using UnityEngine;

// 玩家状态基类：缓存玩家依赖，并处理所有玩家状态共有的动画与冲刺输入。
public abstract class PlayerState : EntityState
{
  protected Player player;
  protected PlayerInputSet input;

  // 状态对象不是 MonoBehaviour，因此需要从 Player 手动取得 Animator、刚体和输入对象。
  public PlayerState(Player player, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
  {
    this.player = player;
    anim = player.anim;
    rb = player.rb;
    input = player.input;
  }
  public override void Update()
  {
    base.Update();

    // yVelocity 驱动 Animator 中的上升/下落混合或过渡条件。
    anim.SetFloat("yVelocity",rb.linearVelocity.y) ;

    // 冲刺属于通用高优先级输入，所以在每个玩家状态的公共 Update 中检测。
    if(input.Player.Dash.WasPressedThisFrame() && CanDash())
      stateMachine.ChangeState(player.dashState);
    
  }
  private bool CanDash()//能否冲刺
  {
    // 当前规则：贴墙时禁止由公共入口冲刺，冲刺状态本身也不能重复进入。
    if(player.wallDetected)
       return false;
    if(stateMachine.currentState == player.dashState)
       return false;   

    return true;   
  }

}
