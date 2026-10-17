using System.Collections.Generic;
using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    public Collider2D[] targetColliders;

    private Entity_VFX vfx;
    // 仅保存当前攻击的配置引用；清除后回退到原攻击属性。
    private Attack_Definition activeAttackDefinition;
    public float damage = 10;

    [Header("默认攻击冲击")]
    [SerializeField, Range(0, 3)]
    private int defaultImpactLevel = 0;

    [Header("攻击判定")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius = 1;
    [SerializeField] private LayerMask whatIsTarget;

    private void Awake()
    {
        vfx = GetComponent<Entity_VFX>();
    }
    public void SetAttackDefinition(Attack_Definition definition)
    {
        activeAttackDefinition = definition;
    }

    public void ClearAttackDefinition()
    {
        activeAttackDefinition = null;
    }

    private float GetAttackRadius()
    {
        return activeAttackDefinition != null
            ? activeAttackDefinition.HitRadius
            : targetCheckRadius;
    }

    public void PerformAttack()
    {
        // 防止进入处决时上一段普攻的迟到动画事件额外结算伤害。
        var execution = GetComponent<Player_Execution>();
        if (execution != null && execution.IsExecuting)
            return;
        float attackDamage = activeAttackDefinition != null
            ? activeAttackDefinition.BaseDamage
            : damage;

        int attackImpactLevel = activeAttackDefinition != null
            ? activeAttackDefinition.ImpactLevel
            : defaultImpactLevel;

        float attackRadius = GetAttackRadius();

        var player = GetComponent<Player>();
        var reload = player != null && player.attackState != null && player.attackState.IsActive
            ? GetComponent<Player_ReloadProgress>() : null;
        float reloadGain = activeAttackDefinition != null ? activeAttackDefinition.ReloadProgressOnHit : 0;
        var hitTargets = new HashSet<IDamgable>();
        foreach (var target in GetDetectedColliders(attackRadius))
        {
            IDamgable damgable = target.GetComponent<IDamgable>();

            if (damgable == null || (player != null && !hitTargets.Add(damgable)))
                continue;

            Enemy_Health enemyHealth = target.GetComponent<Enemy_Health>();
            bool canGainProgress = reload != null && enemyHealth != null && !enemyHealth.IsDead;
            float beforeHealth = canGainProgress ? enemyHealth.CurrentHealth : 0;
            damgable.TakeDamage(attackDamage, transform, attackImpactLevel);
            // TakeDamage 内已完成击杀回弹；之后按弹仓剩余空间积累装填。
            if (canGainProgress && enemyHealth != null && enemyHealth.CurrentHealth < beforeHealth)
                reload.AddProgress(reloadGain);
            vfx.CreateOnHitVFX(target.transform);
        }
    }

    // 保留原范围查询入口，反击继续使用原判定半径。
    protected Collider2D[] GetDetectedColliders()
    {
        return GetDetectedColliders(targetCheckRadius);
    }

    protected Collider2D[] GetDetectedColliders(float radius)
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, radius, whatIsTarget);
    }

    private void OnDrawGizmos()
    {
        if (targetCheck == null)
            return;

        Gizmos.DrawWireSphere(targetCheck.position, GetAttackRadius());
    }
}
