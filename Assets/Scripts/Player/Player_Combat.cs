using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Combat : Entity_Combat
{
    [Header("地面普攻配置")]
    [Tooltip("第一段地面普攻；未配置时使用原攻击属性和默认冲击等级。")]
    [SerializeField] private Attack_Definition firstGroundAttackDefinition;

    [Tooltip("第二段地面普攻；未配置时使用原攻击属性和默认冲击等级。")]
    [SerializeField] private Attack_Definition secondGroundAttackDefinition;
    [Tooltip("第三段地面普攻；未配置时使用原攻击属性和默认冲击等级。")]
    [SerializeField] private Attack_Definition thirdGroundAttackDefinition;

    [Header("地面招式派生：未配置时沿用固定三连段")]
    [SerializeField] private Attack_MoveDefinition firstGroundMoveDefinition;
    [SerializeField] private InputAction alternativeAttackInput = new InputAction("AlternativeAttack", InputActionType.Button, "<Keyboard>/f");
    public Attack_MoveDefinition FirstGroundMoveDefinition => firstGroundMoveDefinition;
    public bool AlternativeAttackPressed => alternativeAttackInput.WasPressedThisFrame();
    private void OnEnable() => alternativeAttackInput.Enable();
    private void OnDisable() => alternativeAttackInput.Disable();
    private void OnDestroy() => alternativeAttackInput.Dispose();

    public Attack_Definition FirstGroundAttackDefinition => firstGroundMoveDefinition != null &&
        firstGroundMoveDefinition.AttackDefinition != null
        ? firstGroundMoveDefinition.AttackDefinition : firstGroundAttackDefinition;

    // 连击下标与 Animator 保持一致：0、1、2 对应第一、二、三段。
    public Attack_Definition GetGroundAttackDefinition(int comboIndex)
    {
        switch (comboIndex)
        {
            case 0: return firstGroundAttackDefinition;
            case 1: return secondGroundAttackDefinition;
            case 2: return thirdGroundAttackDefinition;
            default: return null; // 未配置或超出已支持段数时使用原攻击属性。
        }
    }

    [Header("枪反属性")]
    [SerializeField] private float counterDuration;
    [SerializeField, Min(0)] private float counterDamage = 10;
    [SerializeField, Range(0, 3)] private int counterImpactLevel = 1;

    // 弹药由 Player 的统一入口扣除；空枪也消耗，命中不重复扣弹。
    public bool CounterAttackPerformed()
    {
        bool hasCounteredSomebody = false;
        var targets = FirstGroundAttackDefinition != null
            ? GetDetectedColliders(FirstGroundAttackDefinition.HitRadius)
            : GetDetectedColliders();
        var hitTargets = new System.Collections.Generic.HashSet<IDamgable>();
        foreach (var target in targets)
        {
            IDamgable damageable = target.GetComponent<IDamgable>();
            Entity_Health health = target.GetComponent<Entity_Health>();
            if (damageable == null || (health != null && health.IsDead) || !hitTargets.Add(damageable))
                continue;

            ICounterable counterable = target.GetComponent<ICounterable>();
            bool canCounter = counterable != null && counterable.CanBeCountered;
            damageable.TakeDamage(counterDamage, transform, counterImpactLevel);
            // 致死后不能重新进入失衡；窗口以命中前的状态为准。
            if (canCounter && health != null && !health.IsDead)
            {
                counterable.HandleCounter();
                hasCounteredSomebody = true;
            }
        }
        return hasCounteredSomebody;
    }

    public float GetFirstGroundAttackDamage() => FirstGroundAttackDefinition != null
        ? FirstGroundAttackDefinition.BaseDamage : damage;

    public Enemy_ExecutionTarget FindExecutionTarget()
    {
        var targets = FirstGroundAttackDefinition != null
            ? GetDetectedColliders(FirstGroundAttackDefinition.HitRadius) : GetDetectedColliders();
        Enemy_ExecutionTarget nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (var collider in targets)
        {
            var candidate = collider.GetComponent<Enemy_ExecutionTarget>();
            if (candidate == null || !candidate.CanBeExecuted)
                continue;
            float distance = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = candidate;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    public float GetCounterDuration() => counterDuration;
}
