# 独立处决 Animator 状态 v0.1

## 目的
已有 PlayerExecutionState 负责处决逻辑，但原动画使用 CounterAttack 参数。本版让处决拥有独立 Animator 入口，为以后按武器、空仓选择专用动作预留清晰位置。

## 实际改动
- 玩家当前使用 Assets/Animations/Animator_Controllers/Animator.controller。新增默认 false 的 Bool 参数 Execution，以及 Base Layer 下的 Player_Execution 状态。
- Entry 在 Execution=true 时选择 Player_Execution；该状态在 Execution=false 时走 Exit。退出不等待动画 Exit Time，过渡时长为 0。沿用项目原有 Entry/Exit 结构。
- Player_Execution 的 Motion 指向新资源 Assets/Animations/Player_Animations/Player_Execution_01.anim。复制现有枪反姿势作为占位，两个相同 Sprite 关键帧使其覆盖 0.8 秒，关闭循环且没有动画事件。
- Execution_Definition 的默认参数、Execution_01.asset 的保存值和 Player.cs 的处决状态构造参数统一为 Execution。
- PlayerExecutionState 不再直接写 CounterAttackperformed，按配置启用自己的参数。正常 Exit 仍由 EntityState 清掉对应 Bool。
- Player_Execution.EndExecution 也清掉本次动作保存的动画参数，覆盖玩家或处决组件禁用时直接清理、未走状态 Exit 的情况。
- 原有 Animator 状态与过渡的文档内容保持原样；只改参数列表和 Base Layer 状态机文档，并追加三个文档。

## 动作链路与知识点
E → TryStartExecution → TryPrepare 保存配置与占用目标 → FSM 切入 PlayerExecutionState → Enter 开启 Execution Bool → Animator Entry 选择 Player_Execution。
C# 状态决定动作规则、锁定和时序；Animator 决定当前显示的动画。Bool 参数是条件，Animator State 是实际播放 Motion 的节点，两者是不同对象。
第 0.4 秒仍由 TickExecution 结算伤害；第 0.8 秒由 C# 切回 Idle，清掉 Execution，再释放双方锁定。Animation Event 目前不驱动处决结算，避免两套入口重复扣血。
EndExecution 中额外清理参数是为了处理禁用生命周期；它与正常 Exit 重复写 false 是安全的，原 IsExecuting 保护仍避免重复释放。
画面当前仍是原枪反姿势占位，本版完成动画状态隔离，不代表已有新美术动作。

## 原规则
总时长 0.8 秒、0.4 秒结算、第一段普攻伤害乘 5、冲击 0、双方行动限制和玩家动作无敌均保留。
处决未击杀不回弹，处决击杀回一发且不增加普通击杀计数；非处决累计三名回一发。
近战装填进度与蓝条继续按上一版规则运行，处决不增加近战装填。

## 自动检查
八个实际写入文件与候选哈希一致。完整脚本编译通过，0 错误；编译日志另行保存在回退包中。
419 项隔离断言通过，包含原有战斗功能，以及独立参数的进入/退出、枪反转处决、0.4/0.8 秒时序、旧事件不提前结算、玩家/组件禁用、目标失效、死亡及配置快照检查。
Animator 静态结构检查通过：文档 ID 唯一，本地引用完整，原状态与过渡保持，Execution 类型为 Bool，Entry/Exit 条件正确，Motion GUID 与新动画一致，关键帧覆盖 0.8 秒且无循环/事件。
隔离适配器和静态检查不会运行真实 Unity Animator、物理、协程或渲染；Unity 导入及 Play Mode 仍需手动验收。本次未启动同工程的第二个后台 Unity。

## Unity 验证步骤
1. 等待导入编译，确认 Console 无新增错误，打开 SampleScene。
2. 选 Player 的 Animator 子对象并打开 Animator 窗口，确认 Controller 为 Animator。Parameters 中应有 Bool Execution；Base Layer 应有 Player_Execution，图中位于枪反状态下方。
3. 选中 Player_Execution：Motion 应为 Player_Execution_01；Entry 进入条件为 Execution=true；到 Exit 的条件为 Execution=false，不勾 Has Exit Time。
4. 进入 Play Mode，枪反制造失衡后按 E：C# 进入 executionState，Animator 高亮 Player_Execution，Execution=true，枪反参数不被处决开启。
5. 确认伤害只结算一次，约 0.4 秒扣血，0.8 秒恢复行动；结束后 Execution=false。右键枪反仍进入原 PlayerCounterAttack 与其成功表现。
6. 处决结算前禁用目标、禁用玩家或处决组件，核对立即/下一帧正确释放、Execution 清零，没有残留锁定。恢复组件后仍可正常战斗。
7. 回归处决击杀仅回一发，普通三杀与近战装填仍正确。死亡能转入 Player_Dead。

## 以后替换专用动画
将 Player_Execution 状态的 Motion 换成专用动画。先在 Execution_01 调整 Duration 与 Hit Time，使动作、结算和结束一致。
如果以后改用动画事件结算，应先将计时与事件汇入同一个结算入口，确保只结算一次，再切换驱动方式。
按武器与空仓选择不同动作属于后续功能；当前仍使用一份 Execution_01 配置。

## 回退
Execution_Animator_v0.1_Backup.zip 包含本次八个文件的 before/after、差异、哈希清单和检查日志。新建动画及 meta 在修改前不存在。
回退前先保存后续工作，再恢复本次修改前的六个既有文件并删除本次新动画及 meta。恢复 Controller 与代码/配置应成套进行，避免参数与连线不匹配。
