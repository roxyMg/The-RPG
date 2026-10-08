# 【第二版骨骼加工工具】在第一版 JSON 上统一 F_ 命名、增加控制骨、调整关节并重排绘制层次。
# 前置：先运行素材准备与 build_rapier_idle_json.py；输出独立的 rapier_idle_v2.json，不覆盖第一版。
# 此工具不在 Unity 运行时执行，也不自动绑定 Player/Enemy Animator 或生成 atlas。
import json
from pathlib import Path


# 【输入源】固定读取第一版文件；不要改成读取自己的 v2 输出，否则重复加工可能产生重复控制骨。
ROOT = Path(r"D:\The RPG\Assets\Character\SpineSource\RapierIdle")
source = json.loads((ROOT / "rapier_idle.json").read_text(encoding="utf-8"))

# 【骨名映射】同一映射必须同步应用于骨名、parent、slot.bone 和动画骨名，否则引用会断开。
bone_names = {
    "pelvis": "F_Hip",
    "torso": "F_Chest",
    "neck": "F_Neck",
    "head": "F_Head",
    "hair": "F_Hair",
    "leg_l": "F_L_Leg",
    "foot_l": "F_L_Foot",
    "leg_r": "F_R_Leg",
    "foot_r": "F_R_Foot",
    "upper_arm_l": "F_L_UpperArm",
    "forearm_l": "F_L_Forearm",
    "hand_l": "F_L_Hand",
    "upper_arm_r": "F_R_UpperArm",
    "forearm_r": "F_R_Forearm",
    "hand_r": "F_R_Hand",
    "weapon": "F_R_Weapon",
}

# 【槽名映射】同步应用于 slots、默认皮肤键和绘制顺序；附件名及图片路径仍沿用第一版。
slot_names = {
    "rapier": "F_Weapon_Rapier",
    "hair_back": "F_Hair_Back",
    "arm_l_upper": "F_L_Arm_Upper",
    "arm_l_fore": "F_L_Arm_Forearm",
    "leg_l": "F_L_Leg",
    "leg_r": "F_R_Leg",
    "hips": "F_Hip",
    "torso": "F_Chest",
    "head": "F_Head",
    "hair_front": "F_Hair_Front",
    "feather": "F_Hair_Feather",
    "arm_r_upper": "F_R_Arm_Upper",
    "arm_r_fore": "F_R_Arm_Forearm",
    "hand_l": "F_L_Hand",
    "hand_r": "F_R_Hand_Grip",
    "collar": "F_Chest_Collar",
}

# 【层级改名】同时修正父节点引用，未列入映射的 root 保持不变。
for bone in source["bones"]:
    bone["name"] = bone_names.get(bone["name"], bone["name"])
    if "parent" in bone:
        bone["parent"] = bone_names.get(bone["parent"], bone["parent"])

# 【整体控制骨】在 root 与 F_Hip 之间插入 F_CTRL，使整个人形可经单个节点统一控制。
source["bones"].insert(1, {"name": "F_CTRL", "parent": "root"})
next(b for b in source["bones"] if b["name"] == "F_Hip")["parent"] = "F_CTRL"

# 【槽位与皮肤引用同步】槽名、骨名和默认皮肤的外层键一起替换，避免附件找不到所属槽位。
for slot in source["slots"]:
    slot["name"] = slot_names[slot["name"]]
    slot["bone"] = bone_names.get(slot["bone"], slot["bone"])

skin = source["skins"]["default"]
source["skins"]["default"] = {slot_names[name]: value for name, value in skin.items()}

# Compact the limbs so the joints overlap cleanly, while keeping every segment separate.
# 【关节对齐】分别缩小上臂、前臂和手的附件并修正局部 y 偏移，不合并图片或改动骨架父子关系。
for slot in ("F_L_Arm_Upper", "F_R_Arm_Upper"):
    attachment = next(iter(source["skins"]["default"][slot].values()))
    attachment.update({"scaleX": 0.38, "scaleY": 0.38, "y": -61})
for slot in ("F_L_Arm_Forearm", "F_R_Arm_Forearm"):
    attachment = next(iter(source["skins"]["default"][slot].values()))
    attachment.update({"scaleX": 0.36, "scaleY": 0.36, "y": -58})
for slot in ("F_L_Hand", "F_R_Hand_Grip"):
    attachment = next(iter(source["skins"]["default"][slot].values()))
    attachment.update({"scaleX": 0.28, "scaleY": 0.28, "y": -27})

# 【装饰与武器姿态】调整羽毛和长剑附件尺寸/位置，再修正武器骨的初始旋转。
next(iter(source["skins"]["default"]["F_Hair_Feather"].values())).update(
    {"scaleX": 0.24, "scaleY": 0.24, "x": -45, "y": 90}
)
next(iter(source["skins"]["default"]["F_Weapon_Rapier"].values())).update(
    {"scaleX": 0.32, "scaleY": 0.32, "x": 210, "y": 0}
)
next(b for b in source["bones"] if b["name"] == "F_R_Weapon")["rotation"] = -72

# Back limbs and hair first, body in the middle, forearms/hands/weapon in front.
# 【绘制层次】后发/后肢在底层，躯干居中，前臂/手/武器靠前；列表需覆盖现有全部槽位。
order = [
    "F_Hair_Back",
    "F_R_Leg",
    "F_L_Leg",
    "F_L_Arm_Upper",
    "F_R_Arm_Upper",
    "F_Hip",
    "F_Chest",
    "F_Head",
    "F_Hair_Front",
    "F_Hair_Feather",
    "F_L_Arm_Forearm",
    "F_R_Arm_Forearm",
    "F_Weapon_Rapier",
    "F_L_Hand",
    "F_R_Hand_Grip",
    "F_Chest_Collar",
]
slots_by_name = {slot["name"]: slot for slot in source["slots"]}
source["slots"] = [slots_by_name[name] for name in order]

# 【动画引用同步】只改关键帧所属骨名，保留第一版的时间、曲线及呼吸摆动数值。
idle_bones = source["animations"]["idle"]["bones"]
source["animations"]["idle"]["bones"] = {
    bone_names.get(name, name): value for name, value in idle_bones.items()
}
# 这里的 hash 是人工版本标签，不是根据文件内容计算出的校验值。
source["skeleton"]["hash"] = "rapier-idle-layered-v2"

# 【独立输出】保留第一版作为可重复加工的输入，运行时只覆盖同名 v2 文件。
(ROOT / "rapier_idle_v2.json").write_text(
    json.dumps(source, ensure_ascii=False, indent=2), encoding="utf-8"
)
print(ROOT / "rapier_idle_v2.json")
