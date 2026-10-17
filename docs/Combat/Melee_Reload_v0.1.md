# 近战装填进度 v0.1

## 目的
把有效近战命中变成可保留的补弹进度，利用已有 HUD 蓝条反馈积累过程，减少中断连段时白打的感觉。本版与已有击杀补弹并存，后续平衡需同时考察两条补给来源。

## 已实现规则
- 第一段、第二段、第三段地面普攻每名有效受伤的敌人分别增加 10、10、15。
- 第三段同时命中三名敌人可增加 45；同一敌人的多个 Collider 在本次玩家近战判定中只伤害和计数一次。
- 满 100 回复一发杀伤弹并保留余量；95 加 15，补一发后剩 10。高数值可以在有空间时逐发结算，已经获得的余量继续保留。
- 中断、Dash、受伤、换目标不清空进度，不设置时间衰减。新开局进度归零。
- 满仓暂停新积累，已有进度保留；开枪留出空位后可继续积累。
- 空挥、尸体、宝箱、非敌人血量、伤害被无敌拒绝、零伤害均不增加。
- 枪反和处决不增加命中装填。本版只接已有地面普攻：跳攻动画目前没有 AttackTrigger 命中事件。
- 已有非处决累计击杀三名回一发、处决击杀回一发的规则继续生效。
- 同次致死命中先完成击杀回弹，再根据当时弹仓是否还有空位增加装填进度。两种奖励可以在有空间时同次发生；若击杀奖励先填满，则本次命中不增加装填，已有进度保持。

## 分类与配置
Assets/Scripts/Player/Resources/Player_ReloadProgress.cs 独立管理命中装填进度。Player_LethalAmmo 仍管理实际杀伤弹和击杀计数，推进弹仍独立。
每招数值：Assets/Data/Combat/Attacks/Rapier/Rapier_Thrust_01、02、03.asset 的 Reload Progress On Hit，分别保存 10、10、15。配置为 0 表示该攻击不提供装填。
运行时观察：SampleScene → Player → Player_ReloadProgress → Reload Progress。
HUD 配置：UI_PlayerHUD 的 Reload Progress、Reload Progress Fill、Reload Progress Background 已绑定，Reload Flash Duration 默认为 0.18 秒。
原 Reserved_BlueBar 改为 ReloadProgress_Background，保留其贴图、位置和半透明底色；新增子对象 ReloadProgress_Fill，水平填充从左至右。

## 代码知识与链路
Attack_Definition 的序列化字段提供可调命中奖励，状态仍按当前招式选择原攻击配置。
Entity_Combat.PerformAttack 在玩家当前地面攻击状态内读取奖励，使用 HashSet 避免同一次判定重复处理同一伤害对象。对 Enemy_Health 比较扣血前后数值，实际减少才通知资源组件；致死命中也属于有效伤害。
Player_ReloadProgress.AddProgress 使用 while 在每满 100 时减去 100 并补一发，避免把余量直接清零。满仓及禁用组件时提前 return，不影响其他战斗逻辑。
RewardSequence 只在命中装填实际补弹时递增。UI_PlayerHUD 在 LateUpdate 读取归一化进度设置 Image.fillAmount，并观察序号变化，将整条背景短暂点亮。即使进度已经回到余量，背景仍可显示完成提示；击杀补弹不触发装填闪光。
HUD 不调用补弹或增加进度逻辑，只刷新蓝条和原弹巢。FSM、Animator、血条、枪反、处决和击杀补弹代码本次没有修改。

## 自动检查
九个实际写入文件与候选哈希一致。场景共有 215 个唯一 document ID，只修改四个既有文档、追加五个文档；本地对象引用完整。
使用项目 Unity 编译参数完成脚本编译：0 错误、6 项既有警告。396 项隔离断言通过，包含既有战斗功能回归以及装填规则、多人/多 Collider、余量、满仓、命中拒绝、中断保留、击杀结算顺序、HUD 填充和闪光计时。
隔离适配器不会运行 Unity 物理、Animator、实际协程或 Canvas 渲染，实际 Game 视图表现与完整 Play Mode 尚待用户验证。当前编辑器已在运行，本次没有启动同工程的第二个后台 Unity。

## Unity 验证
1. 等待导入编译，确认 Console 无新增错误，载入磁盘上的 SampleScene，进入 Play Mode。先用枪反消耗弹药，留出空位。
2. 对单名存活敌人分别完成三段有效命中，蓝条应按 10、10、15 增加；空挥不增加。
3. 第三段同时有效命中三名敌人时可加 45；同时查看敌人真实扣血情况。当前弹仓空间不足时可能会提前填满并暂停后续积累。
4. 在 Play Mode 的 Player_ReloadProgress 中观察/临时调整 Reload Progress 为 95，确保下一次有效命中来自奖励为 15 的第三段：应补一发，余量为 10，整条蓝色背景短暂变亮，上半弹巢增加一格。
5. 中断连段、Dash、受伤、换目标后检查进度保留。枪反、处决未击杀、尸体及宝箱不增加装填。
6. 满仓时进度保持；再开枪后有效近战继续积累。
7. 普通击杀进度为 2 且弹仓为 2 发时，下一次致死近战先通过击杀补到 3 发，装填进度应保持原值。
8. 核对三名普通击杀回弹和处决击杀回弹仍正确，上半杀伤弹、下半推进弹与血条显示正常。
9. 检查蓝条实际裁切方向、位置及 0.18 秒完成闪光是否清晰，原 HUD 无遮挡。测试后退出 Play Mode，避免保存临时测试数值。

## 回退备份
Melee_Reload_v0.1_Backup.zip 保存九个文件的修改前/修改后、差异、哈希清单及检查日志。新增 Player_ReloadProgress.cs 和 meta 在修改前不存在。
回退时先保存后续工作，按清单恢复既有文件并删除本次新增脚本及 meta；恢复场景会同时去掉新填充层和新组件引用。
