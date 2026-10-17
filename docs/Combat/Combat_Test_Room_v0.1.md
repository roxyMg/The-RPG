# 暮色庭院 · 战斗测试房 v0.1

建立日期：2026-10-17；交付复核：2026-10-18。用途：独立战斗实验；不作为正式关卡。

## 打开与操作

- Unity 6000.5.7f1：打开 `Assets/CombatTest/Scenes/CombatTestRoom.unity`，点击 Play。也可使用菜单 `Tools > Combat Test > Open Test Room`。
- Windows 测试包与素材归档：`D:\BaiduNetdiskDownload\CombatTestRoom_v0.1`。构建、运行结果以该目录 `QA` 下的实际记录为准。
- A / D 移动，空格跳跃，左键普攻，F 派生，右键枪反，Shift 推进冲刺，E 处决。
- R 或右下角按钮重开。战斗中、死亡后、胜利后和处决中都能重新开始。
- Esc 暂停 / 继续；暂停时也能按 R 重开。
- 重开会销毁并重新创建这一回合的玩家和敌人，恢复生命、弹药、装填进度、动作状态和处决占用。环境保持不变，不写存档。

## 精英：墓地守卫

生命 240，玩家生命 100。固定按横斩 → 突进 → 重砸循环，便于反复比较。开局留有两秒准备时间，敌人接近到相应距离才起招。攻击开始后锁定朝向，不在前摇末尾突然追踪玩家。

| 招式 | 前摇 | 有效期 | 收招 | 伤害 | 应对 |
| --- | --- | --- | --- | --- | --- |
| 横斩 | 0.70 秒 | 0.18 秒 | 0.80 秒 | 12 | 金色地面标记，前摇可枪反；后退或冲刺躲开 |
| 突进 | 0.90 秒 | 0.42 秒 | 0.95 秒 | 16 | 金色路线，前摇可枪反；跳跃或冲刺穿过 |
| 重砸 | 1.15 秒 | 0.22 秒 | 1.20 秒 | 22 | 红色范围，不可枪反；跳跃躲避低位冲击 |

每招对每个目标最多结算一次。金色可反击提示与实际可枪反时段同步关闭。成功枪反进入 2.8 秒失衡，可接现有 E 处决。使用原玩家伤害、推进无敌、弹药及近战装填系统。

三招当前复用原骷髅攻击图，通过播放节奏、位移、范围和颜色区分，是测试占位表现；未宣称具有三套新绘制动作。测试专用 Animator Override 移除了原动画中的伤害事件，避免自定义判定和旧事件重复扣血。

## 美术与来源

环境采用 `D:\BaiduNetdiskDownload\GothicVania-town-files` 中的像素背景、地砖、屋舍、路灯、木箱与木桶。作者 Luis Zuno / @ansimuz；原包 `public-license.txt` 声明美术为 public domain。许可证副本保留在 `Assets/CombatTest/Art/LICENSE.txt`。本次未采用音乐。

角色复用工程当前骑士与骷髅素材。精英适度放大并使用旧金色，维持项目像素、暗色奇幻风格。环境图使用 Point 过滤、16 PPU、不压缩，不搬迁或替换旧素材。`Marker.png` 是工具生成的纯白单像素，仅作为地面判定提示。

## 隔离和回退

开始前工作区已有 97 个文件的新增/修改，已整体保存为独立基线提交，包含此前未跟踪的战斗脚本、数据、场景及文档。

- 工作分支：`codex/combat-test-room`。
- 测试交付标签：`combat-test-v0.1-20261018`。
- 原工作区基线：`e4a321df4d62523768c12cc37552f5c2e6b83799`。
- 基线标签：`combat-test-baseline-20261017`。
- 基线源码 ZIP：`D:\BaiduNetdiskDownload\CombatTestRoom_v0.1\Rollback\before-combat-test-e4a321d.zip`。
- `main` 原分支保留，但它不包含开始任务时那些未提交内容；需要恢复到本次开始前的完整状态时，使用上述基线标签。

不丢失测试版本的回退方式：先退出 Unity Play Mode，保存后续改动，确认 `git status --short` 为空，再运行：

```powershell
git switch -c rollback/combat-test-baseline combat-test-baseline-20261017
```

恢复测试版本：

```powershell
git switch codex/combat-test-room
```

若同名回退分支已存在，使用 `git switch rollback/combat-test-baseline`。不要用 `reset --hard` 或 `clean` 处理后续工作。ZIP 应解压到新目录查看/恢复，避免直接覆盖现有工程。

测试实现集中于 `Assets/CombatTest`，只引用现有角色/动画/战斗配置。原 `SampleScene`、共享玩法脚本、原动画控制器及正式构建场景列表不因本次功能修改。测试包构建通过显式场景列表，只包含测试房。

## 验证与重现

`CombatTestBuilder.Create` 从基线场景提取测试角色并生成独立场景；`CombatTestBuilder.Build` 构建测试包。已生成的场景和 Prefab 可直接使用，不必重复生成。

回归命令（退出其他打开同工程的 Unity 实例后执行）：

```powershell
& 'E:\Unity\6000.5.7f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'D:\The RPG' -executeMethod CombatTest.Editor.CombatTestSmoke.Run -combatSmoke -logFile 'D:\BaiduNetdiskDownload\CombatTestRoom_v0.1\smoke.log'
```

测试器只在编辑器且明确传入 `-combatSmoke` 时运行，独立构建不包含它。覆盖初始生命/站地、三招伤害及反击窗口、原玩家枪反查询、失衡与处决占用、处决中重开、推进无敌、胜败、尸体消散、连续重开、弹药和装填复位、暂停后重开。完成后输出 `QA/smoke-results.txt` 并退出；失败有详细记录。

最终验收结果见同目录 `Combat_Test_Room_v0.1_QA.md`。
