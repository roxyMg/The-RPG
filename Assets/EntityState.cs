using UnityEngine;

public class EntityState
{
  protected Player player;
  protected StateMachine stateMachine;
  protected string animBoolName;
  protected Animator anim;
  protected Rigidbody2D rb;
  public EntityState(Player player, StateMachine stateMachine, string animBoolName)
  {
    this.player = player;
    this.stateMachine = stateMachine;
    this.animBoolName = animBoolName;
    anim = player.anim;
    rb = player.rb;
  }
  public virtual void Enter()
  {
    anim.SetBool(animBoolName, true);
    // Code to execute when entering the state
  }
  public virtual void Update()
  {
    Debug.Log("Updating state" + animBoolName);
    // Code to execute on each frame while in the state
  }
  public virtual void Exit()
{
    anim.SetBool(animBoolName, false);
    // Code to execute when exiting the state
  }
}
