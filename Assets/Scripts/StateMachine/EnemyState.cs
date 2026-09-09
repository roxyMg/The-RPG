using UnityEngine;

// 敌人状态基类：把 Enemy 上的公共组件与动画速度参数提供给具体敌人状态。
public class EnemyState : EntityState
{
    protected Enemy enemy;

    // EntityState 不直接持有实体引用，因此在这里完成敌人组件绑定。
    public EnemyState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.enemy = enemy;
        anim = enemy.anim;
        rb = enemy.rb;
    }
    public override void Update()
    {
        base.Update();
        // Animator 必须存在名为 AnimSpeedMultiplier 的 Float 参数。
        anim.SetFloat("AnimSpeedMultiplier", enemy.AnimSpeedMultiplier); // Set the animation speed multiplier based on the enemy's AnimSpeedMultiplier
       
    }

}
