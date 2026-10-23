

## Codely Structured Memories

### User

### Feedback

### Project
- [2026-10-23 23:00:22] The-RPG 项目原为 Unity 6000.5.7f1 创建（git 提交 d515df4 保留 Unity 6 配置可回滚），2026-10-23 已迁移到 Tuanjie 1.6.13（2022.3.61t14）并验证正常运行：manifest.json 固定 Tuanjie 包集（URP 14.1.0 内置、inputsystem 1.14.1、test-framework 1.4.6、cinemachine 3.1.7、timeline 1.8.12、ugui 1.0.0、cn.tuanjie.codely.bridge），删除 packages-lock.json 后编辑器重解析成功；移除 Unity 6 独有包（2d.animation 15.x、aseprite、psdimporter、spriteshape、tilemap.extras、tooling、ai.assistant、ai.inference、collab-proxy、multiplayer.center、visualscripting、modules.accessibility/adaptiveperformance/physicscore2d/vectorgraphics——AssetDatabase 扫描确认项目 0 处使用，无 asmdef）。代码修复：Rigidbody2D.linearVelocity→velocity 37 处（Tuanjie 无 linearVelocity，反射验证过）、Entity_VFX.cs 删 using Unity.InferenceEngine（死引用）、FindObjectsByType<T>() 无参 3 处补 FindObjectsSortMode.None；FindFirstObjectByType 在 Tuanjie 只有无参/FindObjectsInactive 重载，不能传 sortMode。packages.tuanjie.cn 不提供 Unity 6 版本号（inputsystem 1.20.0、test-framework 1.7.0 解析失败）但提供 1.14.1/1.4.6。验证结论：编译 0 错误 0 警告，SampleScene Play Mode 5 秒 0 异常，URP 管线资产正常加载渲染。

### Reference

