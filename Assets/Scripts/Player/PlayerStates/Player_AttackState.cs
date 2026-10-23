using UnityEngine;

// 地面普攻状态：管理连击段数、攻击位移、输入缓存，以及动画结束后的去向。
public class PlayerAttackState : PlayerState
{
    private Player_Combat combat;
    private Attack_MoveDefinition currentMove, queuedMove, pendingMove;
    private bool useMoveGraph, transitionRequested;
    public Attack_MoveDefinition CurrentMove => currentMove;
    // 只读调试信息，UI 不通过这些属性修改战斗状态。
    public Attack_MoveDefinition QueuedMove => queuedMove;
    public bool HasQueuedAttack => useMoveGraph ? queuedMove != null : comboAttackQueued;
    public bool IsActive => stateMachine.currentState == this;
    public bool UsesMoveGraph => useMoveGraph;
    public bool IsTransitionRequested => transitionRequested;
    public int AnimationIndex => comboIndex;
    private float attackStartTime;
    public float AttackElapsedTime => IsActive ? Mathf.Max(0, Time.time - attackStartTime) : 0;
    public bool UsesBranchWindow => useMoveGraph && currentMove != null && currentMove.BranchWindowEnabled;
    public bool IsBranchWindowOpen => IsActive && !transitionRequested &&
        (!UsesBranchWindow || (AttackElapsedTime >= currentMove.BranchWindowStart &&
                              AttackElapsedTime <= currentMove.BranchWindowEnd));
    private string lastInputName = "无", lastInputResult = "尚未输入";
    public string LastInputDescription => lastInputName == "无"
        ? "无" : lastInputName + "（" + lastInputResult + "）";
    private void RecordDebugInput(string name, bool accepted)
    {
        lastInputName = name;
        lastInputResult = !IsBranchWindowOpen ? "窗口外输入" : accepted ? "有效" : "无对应派生";
    }
    // 仅由帧末切换在 ChangeState 前设置；外部中断不会遗留待接招式。
    public void PrepareNextMove(Attack_MoveDefinition move) => pendingMove = move;
    private float attackVelocityTimer;//用于实现攻击时可移动
    private float lastTimeAttacked;// 记录上一段攻击退出时刻，用于判断连击是否已经超时。
    private bool comboAttackQueued;//预输入？
    private int attackDir;//攻击时方向
    private const int FirstComboIndex = 0;//攻击连段从attack0开始(常量变量记得首字母大写)
    // comboIndex 同时作为 Animator 段数和 attackVelocity 数组下标。
    private int comboIndex = 0;
    private int comboLimit = 0;

    public PlayerAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat = player.GetComponent<Player_Combat>();
        // 最后一段下标由攻击速度数组长度决定；数组内容需与动画连击段数保持一致。
        if(comboLimit != player.attackVelocity.Length - 1)
        {
            comboLimit = player.attackVelocity.Length - 1;
        }
    }

    public override void Enter()
    {
        base.Enter();
        comboAttackQueued = false;
        // 每次进入派生招式重新计时；不修改 Animator 的播放速度或结束事件。
        attackStartTime = Time.time;
        transitionRequested = false;
        lastInputName = "无";
        lastInputResult = "尚未输入";
        queuedMove = null;
        currentMove = pendingMove != null ? pendingMove : combat.FirstGroundMoveDefinition;
        pendingMove = null;
        useMoveGraph = currentMove != null;
        if (useMoveGraph)
        {
            comboIndex = currentMove.AnimationIndex;
            combat.SetAttackDefinition(currentMove.AttackDefinition != null ? currentMove.AttackDefinition : combat.GetGroundAttackDefinition(comboIndex));
        }
        else
        {
            ResetCombIndex();
            combat.SetAttackDefinition(combat.GetGroundAttackDefinition(comboIndex));
        }

        // 有水平输入时锁定本段攻击方向，否则沿用当前面朝方向。
        // 当前强制转换会把绝对值小于 1 的模拟摇杆输入截断为 0。
        if(player.moveInput.x != 0)//根据输入调整攻击朝向
           attackDir = ((int)player.moveInput.x);//攻击朝向
        else//攻击朝向
           attackDir = player.facingDirection;//攻击朝向
        
        // Animator 必须包含 BasicAttackIndex 整数参数，并按该值选择连击动画。
        anim.SetInteger("BasicAttackIndex", comboIndex);
        ApplyAttackVelocity();//调整攻击时的位移
    }

    public override void Update()
    {
        base.Update();

        // 公共更新已切走时，停止执行旧攻击逻辑。
        if (stateMachine.currentState != this)
            return;

        HandleAttackVelocity();

        if (!transitionRequested)
        {
            if (input.Player.Attack.WasPressedThisFrame())
                QueueNextAttack();
            // 同帧两种输入优先 F；不同帧以最后一个有效派生输入为准。
            if (useMoveGraph && combat.AlternativeAttackPressed)
            {
                bool accepted = IsBranchWindowOpen && currentMove.AlternativeFollowUp != null;
                RecordDebugInput("F", accepted);
                if (accepted)
                    queuedMove = currentMove.AlternativeFollowUp;
            }
        }

        // triggerCalled 由攻击动画末尾的 Animation Event 设置。
        if(triggerCalled)
           HandleStateExit();
    }

    public override void Exit()
    {
      // 状态退出时清除当前引用，不影响 Inspector 中保存的配置。
      combat.ClearAttackDefinition();
      base.Exit();
      // 旧固定连段退出时推进下标；派生模式由下一招配置决定下标。
      if (!useMoveGraph)
          comboIndex++;
      currentMove = null;
      queuedMove = null;  
      lastTimeAttacked = Time.time;
    }
    private void HandleStateExit()
    {
        // 已缓存下一击时，先关闭 Animator Bool，再延迟到帧末重新进入 Attack。
        if (transitionRequested)
            return;
        if (useMoveGraph)
        {
            if (queuedMove != null)
            {
                transitionRequested = true;
                anim.SetBool(animBoolName, false);
                player.EnterAttackStateWithDelay(queuedMove);
            }
            else
                stateMachine.ChangeState(player.idleState);
            return;
        }
        if(comboAttackQueued)
            {
                transitionRequested = true;
                anim.SetBool(animBoolName, false);
                player.EnterAttackStateWithDelay();
            }
            else
               stateMachine.ChangeState(player.idleState);
    }
    private void QueueNextAttack()
    {
        RecordDebugInput("左键", useMoveGraph ? currentMove.NormalFollowUp != null : comboIndex < comboLimit);
        // 窗口外不写入缓存，也不清掉之前窗口内接受的招式。
        if (!IsBranchWindowOpen)
            return;
        if (useMoveGraph)
        {
            if (currentMove.NormalFollowUp != null)
                queuedMove = currentMove.NormalFollowUp;
            return;
        }
        // 最后一段不再接受连击缓存。
        if(comboIndex < comboLimit)
           comboAttackQueued = true;
    }
    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;

        // 攻击位移窗口结束后只清零水平速度，继续保留重力产生的垂直速度。
        if(attackVelocityTimer < 0)
           player.SetVelocity(0,rb.velocity.y);
    }
    private void ApplyAttackVelocity()
    {   
        // 每段连击使用 Inspector 中 attackVelocity 对应下标的速度。
        Vector2 attackVelocity = player.attackVelocity != null && comboIndex >= 0 && comboIndex < player.attackVelocity.Length
            ? player.attackVelocity[comboIndex] : Vector2.zero;

        attackVelocityTimer = player.attackVelocityDuration;
        player.SetVelocity(attackVelocity.x * attackDir, attackVelocity.y);
    }
    private void ResetCombIndex()
    {
        if(Time.time > lastTimeAttacked + player.comboResetTime)//记得检测连击时间间隔
           comboIndex = FirstComboIndex;

        if(comboIndex > comboLimit)
           comboIndex = FirstComboIndex;
    }
}
