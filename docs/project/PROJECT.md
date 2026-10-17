# 项目总览与模块盘点

盘点日期：2026-10-13。依据：本地工作区，包含当前尚未提交的修改。

入口：[首页](../../README.md) · [任务看板](TASKS.md) · [素材台账](ASSETS.md) · [验收](ACCEPTANCE.md)

## 当前阶段

项目处于功能原型整合阶段。已有移动、战斗、敌人、HUD 和多批美术资源，缺少统一的功能验收、素材采用记录与版本目标。

已确认的工程配置：

- Unity：`6000.5.7f1`，来自 [ProjectVersion.txt](../../ProjectSettings/ProjectVersion.txt)。
- 渲染和输入：包配置包含 URP、2D 与 Input System，来自 [manifest.json](../../Packages/manifest.json)。
- 当前唯一启用的构建场景：[SampleScene.unity](../../Assets/Scenes/SampleScene.unity)，来自 [EditorBuildSettings.asset](../../ProjectSettings/EditorBuildSettings.asset)。
- 当前存在多处未提交的玩法、场景与数据修改。整理基线时应包含必要的新文件和 `.meta`，不能仅保存已有跟踪文件。
- 本次只做静态盘点；没有确认运行稳定性、视觉效果或构建成功。没有发现项目内自编写的测试文件，安装的测试框架不等于已有测试覆盖。

## 本版 demo 的建议目标

以下作为初始排期建议，正式范围尚待项目负责人确认：玩家进入一个小型演示关卡，完成移动、三段普攻、枪反失衡、处决和弹药循环，HUD 显示正确，并能结束演示、重新开始。

本版候选内容：一个演示场景、一种骷髅敌人、一套普攻配置、一种处决、玩家血条和双弹药 HUD、统一的一组环境与反馈素材。

待后续规划：多武器处决、冲击等级 3 的倒地状态、完整背包/任务/存档、大地图与更多敌人。它们目前不进入本轮就绪队列；若是 demo 必需项，先调整范围再排期。

## 模块盘点

“待验证”表示存在代码或配置证据，尚未完成 Unity 实际验收，不表示功能已经完成。

| 模块 | 已观察到的实现 | 当前阶段 / 缺口 | 相关任务 |
| --- | --- | --- | --- |
| 玩家移动 | [Player](../../Assets/Scripts/Player/Player.cs) 与 [玩家状态目录](../../Assets/Scripts/Player/PlayerStates) 包含移动、跳跃、墙滑、墙跳、冲刺 | 已实现，待验证切换与动画；反复启用的输入订阅值得检查 | DEMO-001、002 |
| 状态机与实体基础 | [StateMachine](../../Assets/Scripts/StateMachine/StateMachine.cs)、[Entity](../../Assets/Scripts/Entity/Entity.cs) 负责状态调度、朝向与检测 | 已实现，待验证受击、死亡、动作锁定期间的切换 | DEMO-002、003、005 |
| 普攻与受击 | [Player_Combat](../../Assets/Scripts/Player/Player_Combat.cs)、[Entity_Combat](../../Assets/Scripts/Entity/Entity_Combat.cs)、[Entity_HitReaction](../../Assets/Scripts/Entity/Entity_HitReaction.cs) | 三段地面攻击已有数据；冲击 0/1/2 有对应逻辑，3 仅预留；普攻多碰撞体重复结算待验证 | DEMO-003 |
| 枪反与失衡 | [枪反状态](../../Assets/Scripts/Player/PlayerStates/Player_CounterAttackState.cs)、[Enemy](../../Assets/Scripts/Enemy/Enemy.cs)、[失衡状态](../../Assets/Scripts/Enemy/EnemyStates/Enemy_Skeleto/Enemy_StunnedState.cs) | 进入枪反时检测命中；有反击窗口与失衡逻辑，待验证窗口、动画及致死边界 | DEMO-004 |
| 处决 | [Player_Execution](../../Assets/Scripts/Player/Player_Execution.cs)、[目标组件](../../Assets/Scripts/Enemy/Enemy_ExecutionTarget.cs)、[Execution_01](../../Assets/Data/Combat/Execution_01.asset) | 单配置，默认 E 键；动作借用 CounterAttack 动画参数；目标占用、伤害和释放待验证 | DEMO-005、011 |
| 推进弹 / 杀伤弹 | [推进弹](../../Assets/Scripts/Player/Resources/Player_PropulsionAmmo.cs)、[杀伤弹](../../Assets/Scripts/Player/Resources/Player_LethalAmmo.cs)、[击杀奖励](../../Assets/Scripts/Enemy/Enemy_Health.cs) | 两种独立弹仓，默认各 3 发；推进弹自动回复，杀伤弹通过击杀/处决回复；待验证不重复结算 | DEMO-004、005 |
| 敌人 AI | [Enemy_Skeleton](../../Assets/Scripts/Enemy/Enemy_Skeleton.cs) 与 [敌人状态目录](../../Assets/Scripts/Enemy/EnemyStates/Enemy_Skeleto) | 骷髅巡逻、追击、攻击、失衡和死亡已有状态；待验证地形遮挡与死亡后行为 | DEMO-006 |
| HUD / 小血条 | [UI_PlayerHUD](../../Assets/Scripts/UI/UI_PlayerHUD.cs)、[UI_MiniHealthBar](../../Assets/Scripts/UI/UI_MiniHealthBar.cs)；场景文本有 HUD 对象和组件 | 玩家血条及上下各 3 格弹药已有接入文本；蓝色条用途待定义；待验证绑定与分辨率 | DEMO-007 |
| 宝箱 | [Chest](../../Assets/Scripts/Chest.cs)、[宝箱动画](../../Assets/Animations/Chest)、[美术说明](../../Assets/Art/TreasureChest/README.md) | 受击开箱已有逻辑；掉落尚未设置；新图与原动画是否采用待确认 | DEMO-012 |
| 镜头 / 环境 | [视差脚本](../../Assets/Scripts/Parallax)、场景中的 Cinemachine 配置、两批港口素材 | 视差与镜头已有场景配置；港口素材位于嵌套 Unity/Assets；采用范围及碰撞需整理 | DEMO-008、009 |
| 角色动画与反馈 | [角色资源](../../Assets/Character)、[动画资源](../../Assets/Animations)、[特效资源](../../Assets/Art/VFX) | Spine 有 v1/v2 源文件；正式采用版本、命中特效和枪反提示接入情况待确认 | DEMO-008、010、011 |
| 演示流程 / 交付 | 当前构建列表只有 SampleScene | 本轮未确认死亡重开、完成提示或独立构建；需先明确流程再验收 | DEMO-013、014 |

## 里程碑与退出条件

| 里程碑 | 目标 | 退出条件 |
| --- | --- | --- |
| M0：建立可运行基线 | 记录当前工程能否运行，列出真实阻碍 | DEMO-001 通过；控制台阻断错误已处理；基线证据及版本可追溯 |
| M1：核心玩法闭环 | 现有玩法、弹药与界面可稳定演示 | DEMO-002 至 007 通过；关键状态不会卡死、重复伤害或重复奖励 |
| M2：统一演示版本 | 素材采用明确、演示可完成并重开 | 素材台账更新；DEMO-009、010、011、013 完成；DEMO-012 是否纳入有明确决策 |
| M3：可交付 demo | 脱离编辑器也能完整演示 | DEMO-014 通过；本版必选用例通过；没有未解决 P0；版本与构建路径有记录 |

没有设定截止日期：尚未确认团队人数、可投入时间及目标平台。按里程碑顺序推进，日期在领取任务后填写。

## 待确认的产品决策

| 决策 | 临时处理 | 在哪里定案 |
| --- | --- | --- |
| 演示目标平台、输入设备 | 验收先沿用已有键鼠输入；发布平台未定 | DEMO-001 / 014，记录到 CHANGELOG |
| 角色和环境的最终视觉风格 | 先标记候选资源，不按文件名或新旧时间认定正式版本 | DEMO-008 / 010 |
| 处决击杀是否同时奖励命中与击杀两发弹药 | 代码存在两条奖励路径，先验证事实，再确认设计 | DEMO-005 |
| 蓝色 HUD 条用途、宝箱是否纳入本版 | 保留为待定义项 | DEMO-007 / 012 |
| 演示结束方式与死亡重开方式 | 在现有功能基线稳定后确定最小方案 | DEMO-013 |
