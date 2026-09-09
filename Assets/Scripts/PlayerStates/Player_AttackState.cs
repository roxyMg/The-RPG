using UnityEngine;

// 地面普攻状态：管理连击段数、攻击位移、输入缓存，以及动画结束后的去向。
public class PlayerAttackState : PlayerState
{
    private float attackVelocityTimer;//用于实现攻击时可移动
    // 记录上一段攻击退出时刻，用于判断连击是否已经超时。
    private float lastTimeAttacked;
    

    private bool comboAttackQueued;//预输入？
    private int attackDir;//攻击时方向
    private const int FirstComboIndex = 0;//攻击连段从attack0开始(常量变量记得首字母大写)
    // comboIndex 同时作为 Animator 段数和 attackVelocity 数组下标。
    private int comboIndex = 0;
    private int comboLimit = 0;

    public PlayerAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
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
        // 如果间隔过长或上一段已超过上限，就从第一段重新开始。
        ResetCombIndex();

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
        HandleAttackVelocity();

        if(input.Player.Attack.WasPressedThisFrame())
           QueueNextAttack();

        // triggerCalled 由攻击动画末尾的 Animation Event 设置。
        if(triggerCalled)
           HandleStateExit();
    }

    public override void Exit()
    {
      base.Exit();
      // 每次离开攻击状态都推进段数；下次 Enter 再负责超时或越界重置。
      comboIndex ++;  
      lastTimeAttacked = Time.time;
    }
    private void HandleStateExit()
    {
        // 已缓存下一击时，先关闭 Animator Bool，再延迟到帧末重新进入 Attack。
        if(comboAttackQueued)
            {
                anim.SetBool(animBoolName, false);
                player.EnterAttackStateWithDelay();
            }
            else
               stateMachine.ChangeState(player.idleState);
    }
    private void QueueNextAttack()
    {
        // 最后一段不再接受连击缓存。
        if(comboIndex < comboLimit)
           comboAttackQueued = true;
    }
    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;

        // 攻击位移窗口结束后只清零水平速度，继续保留重力产生的垂直速度。
        if(attackVelocityTimer < 0)
           player.SetVelocity(0,rb.linearVelocity.y);
    }
    private void ApplyAttackVelocity()
    {   
        // 每段连击使用 Inspector 中 attackVelocity 对应下标的速度。
        Vector2 attackVelocity = player.attackVelocity[comboIndex];

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
