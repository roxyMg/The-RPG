using UnityEngine;
using UnityEngine.InputSystem;

// 输入、配置与本次处决会话；动作更新由 PlayerExecutionState 驱动。
public class Player_Execution : MonoBehaviour
{
    [Header("第一种处决配置：后续按武器和空仓选择不同资源")]
    [SerializeField] private Execution_Definition defaultDefinition;
    [Header("处决输入")]
    [SerializeField] private InputAction executionInput = new InputAction("Execution", InputActionType.Button, "<Keyboard>/e");
    private Player player;
    private Player_Combat combat;
    private Enemy_ExecutionTarget target;
    private bool settled;
    private float elapsed, duration, hitTime, executionDamage, savedGravity;
    public bool IsExecuting { get; private set; }
    public string AnimationBoolName { get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();
        combat = GetComponent<Player_Combat>();
    }
    private void OnEnable() => executionInput.Enable();
    private void Update()
    {
        if (executionInput.WasPressedThisFrame())
            player.TryStartExecution();
    }
    private void OnDisable()
    {
        executionInput.Disable();
        EndExecution();
    }
    private void OnDestroy() => executionInput.Dispose();

    // 目前只有一种配置。将来在此根据武器与弹仓选取，不改状态与伤害链路。
    public Execution_Definition GetExecutionDefinition() => defaultDefinition;
    public bool TryPrepare()
    {
        Execution_Definition definition = GetExecutionDefinition();
        if (!isActiveAndEnabled || IsExecuting || definition == null || combat == null)
            return false;
        Enemy_ExecutionTarget nearest = combat.FindExecutionTarget();
        if (nearest == null || !nearest.TryReserve(player))
            return false;
        target = nearest;
        duration = definition.Duration;
        hitTime = definition.HitTime;
        executionDamage = combat.GetFirstGroundAttackDamage() * definition.DamageMultiplier;
        AnimationBoolName = definition.AnimationBoolName;
        settled = false;
        elapsed = 0;
        IsExecuting = true;
        return true;
    }
    public void BeginExecution()
    {
        savedGravity = player.rb.gravityScale;
        player.rb.gravityScale = 0;
        player.rb.velocity = Vector2.zero;
        player.invulnerability.BeginActionInvulnerability();
    }
    // 返回 true 表示动作结束或目标已无法结算。
    public bool TickExecution(float deltaTime)
    {
        if (!IsExecuting)
            return true;
        if (!settled && (target == null || !target.IsReservedBy(player)))
            return true;
        player.rb.velocity = Vector2.zero;
        elapsed += Mathf.Max(0, deltaTime);
        // 先结算再退出，保证大帧跨过两个时点也只结算一次。
        if (!settled && elapsed >= hitTime)
        {
            settled = true;
            // 处决未击杀不回弹；致死奖励统一由 Enemy_Health 通知弹仓。
            target.Health.TakeExecutionDamage(executionDamage, player.transform);
        }
        return elapsed >= duration;
    }
    public void EndExecution()
    {
        if (!IsExecuting)
            return;
        IsExecuting = false;
        // 禁用组件也可能直接到这里，不一定经过状态 Exit。
        player.anim.SetBool(AnimationBoolName, false);
        if (target != null)
            target.Release(player);
        target = null;
        player.invulnerability.EndActionInvulnerability();
        Entity_Health health = player.GetComponent<Entity_Health>();
        if (health == null || !health.IsDead)
        {
            player.rb.gravityScale = savedGravity;
            player.rb.velocity = Vector2.zero;
        }
    }
}
