using UnityEngine;

// 骷髅攻击状态的占位实现；目前只继承 EnemyState 的动画开关和速度倍率同步。
public class Enemy_AttackState : EnemyState
{
    public Enemy_AttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }
}
