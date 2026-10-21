# 素材台账与目录规范

盘点日期：2026-10-13。入口：[首页](../../README.md) · [任务](TASKS.md) · [验收](ACCEPTANCE.md)

阶段顺序：候选 → 采用版确认 → 导入配置确认 → 场景/动画接入 → 运行验收通过。每个阶段都需要证据；文件存在并不代表在用。

## 当前素材台账

2026-10-19 更新 ART-11：月潮港第二版。用内置 ImageGen 重新生成 Architecture（建筑）、Kit（道具与地表）、Stone（实心墙面）、Panorama（海天远岸）四张源图；完整实际提示词在 `Assets/Scenes/HarborVillageV2/Art/Source/GenerationPrompts.md`。切片去除透明留白及邻接建筑碎片，地面纹理全不透明；Point / 16 PPU / 无 mipmap / 无压缩，每种精灵使用独立 URP 2D 材质。旧版素材移到 `.utmp/HarborV1Archive`。角色完整沿用 SampleScene 的原 Player，不再只借用动画帧。见 [场景说明](../../Assets/Scenes/HarborVillageV2/README.md)。

2026-10-17 测试房专用素材：从用户指定的 `D:\BaiduNetdiskDownload\GothicVania-town-files` 选取背景、地砖、屋舍、路灯、箱桶，当前存放于 `Assets/Scenes/CombatTest/Art`；Point / 16 PPU，无压缩。美术作者 Luis Zuno，原 public domain 许可证随文件保留。玩家与精英引用现有骑士/骷髅，仅生成测试专用 Prefab 与去除旧伤害事件的动画覆盖。详见 [测试版来源和采用关系](../Combat/Combat_Test_Room_v0.1.md)。后续素材来源目录沿用 `D:\BaiduNetdiskDownload`。

| ID / 类别 | 当前路径 | 版本与用途 | 当前状态 | 来源 / 待补信息 | 关联任务 |
| --- | --- | --- | --- | --- | --- |
| ART-01 玩家动画 | [Character/Knight](../../Assets/Character/Knight)、[Player_Animations](../../Assets/Animations/Player_Animations)、[Player_A.controller](../../Assets/Animations/Animator_Controllers/Player_A.controller) | 有 Warrior_Sheet-Effect 和移动/战斗片段 | 文件存在；当前实际采用图与 Controller 待核对 | 来源、比例、引用者及正式采用版待补 | DEMO-008、010 |
| ART-02 骷髅 | [Character/Skeleton](../../Assets/Character/Skeleton)、[Skeleton_Animations](../../Assets/Animations/Skeleton_Animations)、[Enemy_Skeleton.controller](../../Assets/Animations/Animator_Controllers/Enemy_Skeleton.controller) | 攻击、行走、受击、死亡等图与部分动画 | 文件存在；运行待验收 | 原始来源、实际使用片段和缺少动画待补 | DEMO-006、008 |
| ART-03 细剑待机源 | [SpineSource/RapierIdle](../../Assets/Character/SpineSource/RapierIdle) | rapier_idle 与 rapier_idle_v2 的 .spine/.json，以及拆分图片 | 两版共存；采用版及 Unity 接入未确认 | 来源、导出配置和正式版待补；[tools](../../tools) 中有构建/准备脚本 | DEMO-008、010 |
| ART-04 玩家 HUD | [Art/UI/Player](../../Assets/Art/UI/Player) | Components_v1 与 NoNumbers_v1 | 场景文本已有 HUD 对象与 UI_PlayerHUD；实际用图和效果待验收 | 原图来源、切片、引用者、蓝色条用途待补 | DEMO-007、008 |
| ART-05 敌人血条 | [Art/UI/Enemy](../../Assets/Art/UI/Enemy) | EnemyHealthBar_Components_v1 | 有小血条组件；实际使用素材待确认 | 来源、切片与引用者待补 | DEMO-007、008 |
| ART-06 宝箱 | [Art/TreasureChest](../../Assets/Art/TreasureChest)、[Animations/Chest](../../Assets/Animations/Chest) | 8 帧开箱图与既有动画 | README 记录切片配置并注明未运行验证；新图是否替换原动画待确认 | 现有 README 记载 image_gen 生成；来源记录不代表本轮已做视觉复核 | DEMO-008、012 |
| ART-07 轻命中特效 | [Art/VFX/LightHit](../../Assets/Art/VFX/LightHit)、[OnHitVFX.prefab](../../Assets/Prefab(预制)/OnHitVFX.prefab) | LightHit_8Frames_v1 | 图片与特效 Prefab 存在；引用关系待确认 | 来源、切片、播放速度、非循环和触发者待补 | DEMO-008、011 |
| ART-08 枪反提示 | [Art/VFX/ParryIndicator](../../Assets/Art/VFX/ParryIndicator) | parry_indicator_4frames | 文件存在；提示窗口接入待确认 | 来源、播放时点、锚点、采用版待补 | DEMO-004、008、011 |
| ART-09 港口第一批 | [HarborTileset_Batch01](../../Assets/Building_MidNight/Unity/Assets/HarborTileset_Batch01) | Terrain / Props / Traversal / Atlas 与 Tile_1 资源 | 候选包；包含 Tile 和 Prefab；采用范围待确认 | 来源待补；导入脚本全注释，预设路径与实际目录不同 | DEMO-008、009 |
| ART-10 港口村庄包 | [HarborVillageTileset_16px](../../Assets/Building_MidNight/Unity/Assets/HarborVillageTileset_16px) | Architecture / Decoration / Foreground / Backgrounds 等 | 候选包；有 Clutter 资源及独立视差脚本 | 来源待补；导入脚本全注释，预设路径与实际目录不同 | DEMO-008、009 |

## 玩法配置也需要登记

| 配置 | 当前值 | 验证状态 |
| --- | --- | --- |
| [Rapier_Thrust_01](../../Assets/Data/Combat/Attacks/Rapier/Rapier_Thrust_01.asset) | 伤害 5，半径 1，冲击 0 | 配置文本已确认，实际挂载与结算待 QA-03 |
| [Rapier_Thrust_02](../../Assets/Data/Combat/Attacks/Rapier/Rapier_Thrust_02.asset) | 伤害 10，半径 1，冲击 1 | 同上 |
| [Rapier_Thrust_03](../../Assets/Data/Combat/Attacks/Rapier/Rapier_Thrust_03.asset) | 伤害 20，半径 1，冲击 2 | 同上 |
| [Execution_01](../../Assets/Data/Combat/Execution_01.asset) | 倍率 5，时长 0.8 秒，命中 0.4 秒，动画参数 CounterAttack | 配置文本已确认，实际流程待 QA-05 |

配置值是当前初始值，后续平衡调整同步更新本表。脚本默认值可能被 Inspector 覆盖，测试以实际场景配置为准。

## 目录整理方案

以下是新资源的目标归属；现有资源通过 DEMO-009 等任务分批迁移后再更新台账。

```text
Assets/
  Scripts/           运行脚本，按 Player、Enemy、Entity、Combat、UI 等模块分组
  Data/              可调玩法配置，如 Combat/Attacks、Execution
  Art/               Unity 采用的图片，按 Characters、Environment、UI、VFX、Props 分组
  Animations/        Animation Clip 与 Animator Controller
  Prefabs/           可复用对象（当前旧目录为 Prefab(预制)）
  Scenes/            正式演示场景与测试场景
  Settings/          输入、渲染等工程资源
SourceAssets/        可选：创作源文件、候选版本及导出记录
docs/project/        项目管理文件
tools/               生成、导出与资源整理脚本
```

SourceAssets 是否采用、Spine 源是否移出 Assets，需要先查清 Unity 或工具是否直接引用。目录树是建议，不表示已完成迁移。当前 `Materials(材料)` 等目录仍维持原路径。

## 素材维护规则

1. 新素材记录 ID、来源（作者/资源包/生成方式）、源路径、正式导出路径、版本、引用者和验收状态。无法确定的写“待确认”。
2. 同一用途确认一个正式采用版本。旧版先标“候选/停用”，完成引用核对后再处理。
3. 名称体现对象、动作与版本，例如 `Player_Rapier_Idle_v02`；不要在同一个整理任务里批量修改脚本类型名和序列化字段。
4. 图片记录透明通道、尺寸、PPU、Pivot、过滤、切片、帧序、帧率和循环属性。宝箱等已有定制切片的资源先阅读原说明。
5. Unity 资源移动优先在编辑器中进行；采用脚本移动时必须保留原 `.meta` 和 GUID。先检查场景、Prefab、Controller、Tile 和工具中的路径引用，再移动一批并验证一批。
6. `Library/`、`Logs/`、生成的 `.csproj` 等属于缓存或工程产物，已由 `.gitignore` 覆盖；日常整理围绕 Assets 和创作源文件开展。

## 新素材登记模板

```text
ID / 名称 / 用途：
来源 / 作者 / 生成方式：
创作源路径 / Unity 导出路径：
采用版本 / 候选版本：
导入参数 / 切片 / 动画参数：
引用者（场景、Prefab、Controller、Tile 或脚本）：
当前阶段 / 关联任务：
验收日期 / 版本 / 证据：
```
