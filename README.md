# The RPG · 项目管理入口

**2026-10-17 测试版：** [暮色庭院战斗测试房 v0.1](docs/Combat/Combat_Test_Room_v0.1.md)。独立场景 `Assets/CombatTest/Scenes/CombatTestRoom.unity`，三招精英、胜败与 R 重开；说明中包含基线与安全回退方式。

这是一个 Unity 2D RPG demo。当前 Unity 版本为 **6000.5.7f1**，构建列表中的演示场景是 [SampleScene.unity](Assets/Scenes/SampleScene.unity)。

打开 Unity 项目后加载该场景进入 Play Mode；能否完整运行和构建，以验收记录为准。

## 从这里开始

| 你想了解什么 | 入口 |
| --- | --- |
| 现在有什么、这版 demo 做到哪里 | [项目总览与模块盘点](docs/project/PROJECT.md) |
| 下一步做什么、任务是否完成 | [任务看板](docs/project/TASKS.md) |
| 素材在哪里、哪版在用、是否接入 | [素材台账与目录规范](docs/project/ASSETS.md) |
| 怎么测试、怎样才算完成 | [验收清单与结果记录](docs/project/ACCEPTANCE.md) |
| 最近完成了什么、有哪些决策 | [变更与决策记录](docs/project/CHANGELOG.md) |

**下一步：执行看板 DEMO-001，确认当前场景的运行基线。**

## 日常使用

1. 开始开发前：在看板选一个任务，确认依赖、范围、负责人和验收条件，改为“进行中”。同时进行的任务最多 2 项。
2. 开发中：新想法先进入“待规划”；阻碍当前演示的问题进入 P0，并记录复现步骤。
3. 完成实现：改为“待验收”，在验收文件留下实际测试结果和证据。
4. 验收通过：改为“已完成”，更新变更记录。涉及素材时同步素材台账。

管理文件位于 `docs/project/`，随 Git 保存。源码存在、素材生成、场景挂载、运行验收是不同阶段；只有满足任务验收条件才能关闭任务。

当前文档依据 2026-10-13 的目录、代码、配置和场景文本建立。尚未进行本轮 Unity 运行或构建验收，已有功能统一标记为待验证。
