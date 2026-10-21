# 第二版最终验证 · 2026-10-19

- Unity 6000.5.7f1，正式工程 D:/The RPG，使用 -batchmode -force-d3d11 与 HarborV2.Editor.HarborTestRunner.Run。
- 正式工程编译完成，无 C# 编译错误。日志：工作区 Logs/HarborV2MainCheck.log。
- 自动测试入口采用 60 Hz 仿真帧步长，防止导入/着色器停顿造成自动驾驶停步误差；不改写正式场景或 TimeManager 设置。
- Play Mode 44 项检查全部 PASS，运行错误/异常 0，详见 Playtest.txt。
- 511 个地面采样点全部通过，详见 GroundValidation.txt。
- 原 Player、刚体、碰撞体、战斗/处决、两类弹药、装填、生命及 Animator 共 10 组配置比对通过，详见 PlayerParity.txt。
- 原项目 Assets/Scripts 与隔离验证副本逐文件哈希一致；原角色、动画、输入资源无本次修改。
- 场景、预制体、材质、网格中的 75 个唯一 GUID 均可解析（含 Unity 内置资源），未发现丢失脚本。
- git -c core.whitespace=cr-at-eol diff --check 通过。保留已有 CombatTest 目录迁移、战斗文档及解决方案变更。
- 第一版备份：.utmp/HarborV1Archive；正式场景 GUID 仍为 51d023c280f706f4d9da289fcf29fc17。
- Overview.png 与 Start / Inn / Dock / OverviewRuntime.png 为 Unity 摄像机渲染。摄像机截图不含屏幕空间 HUD；未制作 Windows 独立发布包。
- 最终美术和用户手动试玩仍待验收。本关没有新增敌人 AI，使用原攻击系统的货箱作为可击打对象。
