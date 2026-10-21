# 月潮港 · 最后一班船（第二版）

打开 `Assets/Scenes/HarborVillage.unity` 并 Play，或使用 `Tools > Harbor Village > Play Level`。

## 本次重做

按参考图重新生成建筑、道具、石墙和远岸四张 ImageGen 源图，弃用第一版地图和探索控制器。三处建筑沿连续岸墙排列，低码头直接连接岸墙。主路使用单个闭合 PolygonCollider2D，石墙和地表是连续网格；贴图切片去除外围透明留白，岸墙与码头接头略微重叠，避免拼块裂缝。

玩家从 `SampleScene` 的完整 Player 对象制作成本关预制体，保留原脚本、FSM、Animator、动画事件、传感器、碰撞体、生命和弹药系统、HUD。没有本关专用移动控制器，也不修改项目原始角色脚本。只有调试覆盖层默认关闭。移动速度 8、跳跃初速度 12、冲刺速度 20、冲刺时间 0.25 秒、重力倍率 3.5。参数以当前 SampleScene 为准，重建时逐项比对。

## 玩法

1. 小屋前按 Q 打开宝箱，取回航海凭证。
2. 沿石道到旅店，Q 点亮港灯记录检查点。
3. 在东侧绞盘按 Q 升起栈桥闸门。
4. 走到右侧小船旁按 Q 登船通关。

另有三件可选纪念物和一个可用原攻击系统击打的货箱。坡道旁两级木台通向高处收藏。落水或死亡从检查点生成新的原角色实例，保留任务进度；R 则重置整局。

| 操作 | 按键 |
| --- | --- |
| 移动 / 跳跃 / 冲刺 | A、D / 空格 / 左 Shift |
| 原战斗操作 | 左键攻击、右键枪反、E 处决、F 派生 |
| 关卡交互 | Q（不占用原 E 处决） |
| 全景 / 跟随 | Tab |
| 暂停 / 继续 | Esc |
| 重新开始 | R |

本关没有敌人 AI；保留原战斗能力，货箱可验证实际动画伤害。没有额外爬梯能力，因为原角色没有爬梯状态。

## 验证结果

2026-10-19：正式工程 Unity 6000.5.7f1 编译与 44 项 Play Mode 检查全部通过，运行错误 0；511 个地面采样点与 10 组角色配置比对通过。详见 [最终验证](Preview/Verification.md) 和 [逐项检查](Preview/Playtest.txt)。

## 文件与复现

- `Art/Source`：四张新源图及内置 ImageGen 的完整生成提示词。
- `Art/Sprites`：Unity 编辑器按实际图像位置裁切的生产素材，Point / 16 PPU / 无 mipmap / 无压缩。
- `Prefabs/ProjectPlayer.prefab`：原项目角色的完整场景副本。
- `Editor/HarborBuilder.cs`：重建入口；重建将覆盖本关场景和生成资产，请先保存手工调整。
- `Preview`：全景、游戏摄像机截图、角色参数比对、Play Mode 检查结果。摄像机截图不包含屏幕空间 HUD。
- 第一版已移出 Assets，备份在工作区 `.utmp/HarborV1Archive`，不再导入编译。

使用 Unity 6000.5.7f1，在关闭同工程其他 Unity 实例后可运行：

```text
Unity.exe -batchmode -force-d3d11 -projectPath "<工程路径>" -executeMethod HarborV2.Editor.HarborTestRunner.Run -logFile "<日志路径>"
```

自动检查会进入 Play Mode，结束后主动退出 Unity；不要加 `-quit`。`BuildAndRun` 先重建再检查。最终美术布局和手感仍待用户试玩验收。
