using UnityEngine;

// 轻量有限状态机：Entity 每帧只更新 currentState，具体行为由各状态自行实现。
public class StateMachine
{
  // 只允许状态机内部替换当前状态，外部通过该属性读取状态。
  public EntityState currentState { get; private set; }

  // 设置初始状态时也要执行 Enter，确保动画参数和状态数据完成初始化。
  public void Initialize(EntityState startState)
    {
        currentState = startState;
        currentState.Enter();
    }

    // 切换顺序固定为：退出旧状态 -> 保存新状态 -> 进入新状态。
    public void ChangeState(EntityState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // 由 Entity.Update 调用，把本帧逻辑转交给当前状态。
    public void UpdateActiveState()
    {
        currentState.Update();
    }


}
