using UnityEngine;

// 处决目标负责占用和释放；不替代原敌人 FSM。
[RequireComponent(typeof(Enemy), typeof(Enemy_Health))]
public class Enemy_ExecutionTarget : MonoBehaviour
{
    private Enemy enemy;
    private Enemy_Health health;
    private Player owner;
    private float savedGravity;
    public Enemy_Health Health => health;
    public bool CanBeExecuted => isActiveAndEnabled && enemy != null && enemy.isActiveAndEnabled &&
        health != null && !health.IsDead && enemy.IsStunned && owner == null;
    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        health = GetComponent<Enemy_Health>();
    }
    public bool TryReserve(Player executor)
    {
        if (executor == null || !CanBeExecuted)
            return false;
        owner = executor;
        savedGravity = enemy.rb.gravityScale;
        enemy.CancelKnockback();
        enemy.rb.velocity = Vector2.zero;
        enemy.rb.gravityScale = 0;
        enemy.SetExecutionLock(true);
        return true;
    }
    public bool IsReservedBy(Player executor) => isActiveAndEnabled && enemy.isActiveAndEnabled &&
        owner == executor && health != null && !health.IsDead && enemy.IsStunned;
    public void Release(Player executor)
    {
        if (owner != executor || owner == null)
            return;
        owner = null;
        enemy.SetExecutionLock(false);
        // 死亡刚体与状态由原死亡流程接管，不能覆盖其击飞或重力。
        if (!health.IsDead)
        {
            enemy.rb.gravityScale = savedGravity;
            enemy.rb.velocity = Vector2.zero;
            enemy.ResumeAfterExecution();
        }
    }
    private void OnDisable()
    {
        if (owner != null)
            Release(owner);
    }
}
