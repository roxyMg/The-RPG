# 【第一版骨骼源文件工具】读取 prepare_rapier_idle.py 输出的 PNG，组装静态骨架与两秒待机动画。
# 输出 rapier_idle.json，作为 build_rapier_idle_v2_json.py 的输入；不由 Unity 状态机自动调用。
# 运行会覆盖旧版 JSON；这里只写源 JSON，不负责图片打包或生成 Spine atlas。
import json
from pathlib import Path
from PIL import Image


# 【资源路径】必须与素材准备工具的 ROOT 一致；缺少 images 下的 PNG 时无法读取 attachment 尺寸。
ROOT = Path(r"D:\The RPG\Assets\Character\SpineSource\RapierIdle")
IMAGES = ROOT / "images"


# 【图片附件构造】读取原图像素尺寸，设置附件相对所属骨骼的偏移、旋转和缩放。
# x/y 为局部坐标，rotation 为角度；默认 scale=0.5，未写入的零值属性交由格式默认值处理。
def region(name, x=0, y=0, rotation=0, scale=0.5):
    width, height = Image.open(IMAGES / f"{name}.png").size
    data = {"path": f"images/{name}", "width": width, "height": height}
    if x:
        data["x"] = x
    if y:
        data["y"] = y
    if rotation:
        data["rotation"] = rotation
    if scale != 1:
        data["scaleX"] = scale
        data["scaleY"] = scale
    return data


# 【骨架层级】父骨先于子骨；肢体、头发和武器随各自父节点运动，位置/角度定义初始姿态。
# 骨名还被 slots、idle 动画及 v2 的 bone_names 表引用，修改时需同步这些引用。
bones = [
    {"name": "root"},
    {"name": "pelvis", "parent": "root", "y": 245},
    {"name": "torso", "parent": "pelvis", "y": 112, "length": 115},
    {"name": "neck", "parent": "torso", "y": 112, "length": 55},
    {"name": "head", "parent": "neck", "x": -8, "y": 50, "length": 75},
    {"name": "hair", "parent": "head", "length": 55},
    {"name": "leg_l", "parent": "pelvis", "x": -47, "y": -22, "rotation": -2, "length": 190},
    {"name": "foot_l", "parent": "leg_l", "y": -178, "length": 55},
    {"name": "leg_r", "parent": "pelvis", "x": 40, "y": -20, "rotation": 2, "length": 190},
    {"name": "foot_r", "parent": "leg_r", "y": -178, "length": 55},
    {"name": "upper_arm_l", "parent": "torso", "x": -76, "y": 70, "rotation": 8, "length": 100},
    {"name": "forearm_l", "parent": "upper_arm_l", "y": -105, "rotation": -4, "length": 95},
    {"name": "hand_l", "parent": "forearm_l", "y": -95, "length": 42},
    {"name": "upper_arm_r", "parent": "torso", "x": 78, "y": 70, "rotation": -10, "length": 100},
    {"name": "forearm_r", "parent": "upper_arm_r", "y": -102, "rotation": 8, "length": 95},
    {"name": "hand_r", "parent": "forearm_r", "y": -92, "length": 42},
    {"name": "weapon", "parent": "hand_r", "rotation": -67, "length": 450},
]

# 【绘制槽位】绑定骨骼与默认附件；列表顺序决定初始绘制前后层次，后面的部件覆盖前面的。
slots = [
    {"name": "rapier", "bone": "weapon", "attachment": "rapier_long"},
    {"name": "hair_back", "bone": "hair", "attachment": "hair_back"},
    {"name": "arm_l_upper", "bone": "upper_arm_l", "attachment": "upper_arm_l"},
    {"name": "arm_l_fore", "bone": "forearm_l", "attachment": "forearm_l"},
    {"name": "leg_l", "bone": "leg_l", "attachment": "boot_l"},
    {"name": "leg_r", "bone": "leg_r", "attachment": "boot_r"},
    {"name": "hips", "bone": "pelvis", "attachment": "hips"},
    {"name": "torso", "bone": "torso", "attachment": "torso"},
    {"name": "head", "bone": "head", "attachment": "head"},
    {"name": "hair_front", "bone": "hair", "attachment": "hair_front"},
    {"name": "feather", "bone": "hair", "attachment": "feather"},
    {"name": "arm_r_upper", "bone": "upper_arm_r", "attachment": "upper_arm_r"},
    {"name": "arm_r_fore", "bone": "forearm_r", "attachment": "forearm_r"},
    {"name": "hand_l", "bone": "hand_l", "attachment": "hand_l"},
    {"name": "hand_r", "bone": "hand_r", "attachment": "hand_grip"},
    {"name": "collar", "bone": "torso", "attachment": "collar"},
]

# 【默认皮肤】外层键对应 slot.name，内层键对应 slot.attachment；region 的 name 对应 PNG 文件名。
# 单独调整附件偏移/缩放不会改变骨架层级，便于按裁切后实际尺寸对齐关节。
attachments = {
    "rapier": {"rapier_long": region("rapier_long", x=188, y=-2, scale=0.28)},
    "hair_back": {"hair_back": region("hair_back", x=20, y=35, scale=0.5)},
    "arm_l_upper": {"upper_arm_l": region("upper_arm_l", x=-8, y=-72, rotation=4, scale=0.43)},
    "arm_l_fore": {"forearm_l": region("forearm_l", x=-2, y=-72, scale=0.43)},
    "leg_l": {"boot_l": region("boot_l", x=0, y=-86, scale=0.5)},
    "leg_r": {"boot_r": region("boot_r", x=0, y=-88, scale=0.5)},
    "hips": {"hips": region("hips", y=13, scale=0.5)},
    "torso": {"torso": region("torso", y=10, scale=0.5)},
    "head": {"head": region("head", x=12, y=18, scale=0.5)},
    "hair_front": {"hair_front": region("hair_front", x=19, y=34, scale=0.5)},
    "feather": {"feather": region("feather", x=-52, y=92, rotation=10, scale=0.3)},
    "arm_r_upper": {"upper_arm_r": region("upper_arm_r", x=8, y=-70, rotation=-3, scale=0.43)},
    "arm_r_fore": {"forearm_r": region("forearm_r", x=1, y=-70, scale=0.43)},
    "hand_l": {"hand_l": region("hand_l", x=-3, y=-39, scale=0.36)},
    "hand_r": {"hand_grip": region("hand_grip", x=0, y=-32, rotation=-10, scale=0.35)},
    "collar": {"collar": region("collar", y=92, scale=0.31)},
}

# 【待机动画】时间单位为秒，0 与 2 秒姿态相同以便循环；1 秒为呼吸和轻微摆动峰值。
# translate/rotate 为相对初始骨骼姿态的变化；curve 设置关键帧之间的缓动。
idle = {
    "bones": {
        "pelvis": {"translate": [
            {"time": 0, "x": 0, "y": 0, "curve": [0.25, 0, 0.75, 1]},
            {"time": 1, "x": 0, "y": 2, "curve": [0.25, 0, 0.75, 1]},
            {"time": 2, "x": 0, "y": 0},
        ]},
        "torso": {
            "rotate": [
                {"time": 0, "angle": -0.5, "curve": [0.25, 0, 0.75, 1]},
                {"time": 1, "angle": 1, "curve": [0.25, 0, 0.75, 1]},
                {"time": 2, "angle": -0.5},
            ],
            "translate": [
                {"time": 0, "x": 0, "y": 0, "curve": [0.25, 0, 0.75, 1]},
                {"time": 1, "x": 0, "y": 4, "curve": [0.25, 0, 0.75, 1]},
                {"time": 2, "x": 0, "y": 0},
            ],
        },
        "head": {"rotate": [
            {"time": 0, "angle": 0.5, "curve": [0.25, 0, 0.75, 1]},
            {"time": 1, "angle": -1, "curve": [0.25, 0, 0.75, 1]},
            {"time": 2, "angle": 0.5},
        ]},
        "upper_arm_l": {"rotate": [
            {"time": 0, "angle": -1, "curve": [0.25, 0, 0.75, 1]},
            {"time": 1, "angle": 1, "curve": [0.25, 0, 0.75, 1]},
            {"time": 2, "angle": -1},
        ]},
        "upper_arm_r": {"rotate": [
            {"time": 0, "angle": 1, "curve": [0.25, 0, 0.75, 1]},
            {"time": 1, "angle": -1, "curve": [0.25, 0, 0.75, 1]},
            {"time": 2, "angle": 1},
        ]},
        "weapon": {"rotate": [
            {"time": 0, "angle": -1, "curve": [0.25, 0, 0.75, 1]},
            {"time": 1, "angle": 1, "curve": [0.25, 0, 0.75, 1]},
            {"time": 2, "angle": -1},
        ]},
        "hair": {"rotate": [
            {"time": 0, "angle": -0.5, "curve": [0.25, 0, 0.75, 1]},
            {"time": 1, "angle": 0.7, "curve": [0.25, 0, 0.75, 1]},
            {"time": 2, "angle": -0.5},
        ]},
    }
}

# 【源格式组装】声明的目标版本为 Spine 3.8.99，皮肤使用当前脚本中的 default 字典结构。
# 与 Unity Animator 的 Player/Enemy 状态动画是独立资源，不会因写出文件自动接入角色状态。
data = {
    "skeleton": {"hash": "", "spine": "3.8.99", "x": -360, "y": -40, "width": 720, "height": 700},
    "bones": bones,
    "slots": slots,
    "skins": {"default": attachments},
    "animations": {"idle": idle},
}

# 【输出】写出 UTF-8、可读缩进的 JSON；v2 工具在此文件上进一步改名、对齐和重排。
(ROOT / "rapier_idle.json").write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
print(ROOT / "rapier_idle.json")
