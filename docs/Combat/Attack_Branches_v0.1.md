# 地面攻击派生验证 v0.1

## 当前规则

- 左键起手第一招；第一招中左键接第二招、F 接第三招；第二招左键接第三招；第三招无后续。
- F 仅作为派生测试输入，不会从待机起手，也不影响右键枪反、E 处决。
- 沿用动画结束事件作为接招时机，整个攻击过程允许缓存一个有效派生选择。本版尚无独立派生窗口、输入过期或提前取消窗口。
- 不同帧按键以最后一个有效输入为准；同帧左键与 F 优先 F。未配置该输入的派生时忽略它，不清掉已有有效缓存。
- 不缓存则当前招结束回 Idle；新攻击从根招式开始。完成、中断后不保留旧派生。

## Inspector 调试入口

Player → Player_Combat → 地面招式派生 → First Ground Move Definition，SampleScene 已绑定 Rapier_Move_01。

资源位于 Assets/Data/Combat/Attacks/Rapier：

| 招式节点 | 攻击属性引用 | Animation Index | Normal Follow Up | Alternative Follow Up |
|---|---|---:|---|---|
| Rapier_Move_01 | Rapier_Thrust_01 | 0 | Rapier_Move_02 | Rapier_Move_03 |
| Rapier_Move_02 | Rapier_Thrust_02 | 1 | Rapier_Move_03 | 空 |
| Rapier_Move_03 | Rapier_Thrust_03 | 2 | 空 | 空 |

Attack Definition 控制伤害、半径、冲击；招式节点只引用原资源，不复制数值。Animation Index 对应现有 BasicAttackIndex 和玩家 attackVelocity 数组下标，本版请使用 0、1、2。

Alternative Attack Input 为 Player_Combat 中独立的 Input System InputAction，默认 F，可在组件 Inspector 调整。未手改原自动生成 PlayerInputSet.cs。

First Ground Move Definition 为空时沿用旧固定连段；旧的三个 Ground Attack Definition 字段保留，也供缺失节点攻击属性时回退。

## 代码与设计意图

Attack_MoveDefinition 为 ScriptableObject：用资源引用表示招式关系，同一个第三招既可从第二招进入，也可从第一招的 F 分支进入。

PlayerAttackState 仍是唯一地面攻击 FSM 状态，新增 CurrentMove、queuedMove（缓存选择）与 pendingMove（帧末衔接）。动画结束时把选定节点交给原有帧末协程，再进入同一个攻击状态；无需为每个招式新增 C# 状态或修改 Animator。

transitionRequested 防止动画结束标记持续为 true 时每帧重新启动协程。帧末协程会核对当前仍是攻击状态，避免旧请求覆盖 Dash、枪反或处决；节点在真正切换前才提交，减少中断后的残留。

枪反范围与处决基础伤害仍以第一段普攻为准：启用图后优先读取根招式引用的 Attack_Definition。原伤害接口、敌人 FSM、血条、Dash 和处决流程保持沿用。

## 验证

- 12 个新增或修改文件写入前核对源文件和备份 SHA-256，写入后逐一匹配候选哈希。
- 完整工程脚本编译 0 错误、6 条既有 Enemy 字段序列化警告，247 项隔离断言通过；日志随压缩包保存。隔离断言覆盖原有战斗、枪反、Dash、HUD、处决，以及普通三连、F 分支、最后有效输入、同时输入优先级、无效输入不丢缓存、单次帧末请求、动作中断、伤害/半径/冲击/位移跟随节点、旧配置回退。
- Scene 仅新增根招式引用；原记录与 Animator、动画片段及 Rapier_Thrust 属性资源未修改。新增资源引用完整且 GUID 无冲突。
- 隔离检查使用 Unity 适配器，不能代替实际 Animator、输入、物理和协程验证。

## Unity 验收步骤

1. 等待导入完成，确认 Console 无编译错误；重新加载 SampleScene 后运行。
2. 左键起手，在第一招中再按左键，第二招中再按左键，确认 1 → 2 → 3。
3. 左键起手，在第一招中按 F，确认 1 → 3，跳过第二段。
4. 第一招先左键后 F，应进入第三段；先 F 后左键，应进入第二段。第二招中只按 F 应结束；先缓存左键再按 F，应仍接第三段。
5. 待机按 F 不攻击；第三招不继续派生。第一招缓存后使用 Dash 中断，再起手应回第一招。
6. 可临时把第三招引用属性调成容易辨识的伤害或冲击，确认 F 分支和普通第三段均使用该值；测试后还原。
7. 检查右键枪反、E 处决、杀敌回弹及 HUD 正常。枪反范围跟随根招式的属性，处决伤害仍为根招式基础伤害乘倍率。

## 回退

压缩包包含 before、after、逐文件 diff 和哈希清单。还原原有四个文件，移除新增 Attack_MoveDefinition.cs/meta 与三个 Rapier_Move 资源/meta，可回到改动前。
