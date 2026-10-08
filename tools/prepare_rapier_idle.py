# 【离线素材准备工具】为 RapierIdle 骨骼源素材去绿幕、按部件裁切并输出透明 PNG。
# 联动顺序：本脚本 -> build_rapier_idle_json.py -> build_rapier_idle_v2_json.py。
# 不属于 Unity 运行时；依赖 Pillow、NumPy。运行会覆盖同名输出，不会自动生成骨骼 JSON。
from pathlib import Path
from PIL import Image
import numpy as np


# 【路径配置】ROOT 保存处理后的整张素材，images 保存分离部件；后续 JSON 工具从 images 读取。
ROOT = Path(r"D:\The RPG\Assets\Character\SpineSource\RapierIdle")
OUT = ROOT / "images"
OUT.mkdir(parents=True, exist_ok=True)


# 原始素材位置为本机绝对路径，换电脑或移动图片后需要同步修改此处。
SOURCE = Path(r"C:\Users\王尔成\DeskBox\图片")


# 【绿幕透明化】通过绿色比红/蓝的优势计算透明度，仅降低原 alpha，保持 RGB 颜色不变。
def chroma_key(source_name: str, output_name: str) -> None:
    """Remove the supplied sheets' neon green while preserving the original art."""
    image = Image.open(SOURCE / source_name).convert("RGBA")
    pixels = np.asarray(image).copy()
    rgb = pixels[..., :3].astype(np.float32)
    red, green, blue = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    dominance = green - np.maximum(red, blue)
    # 绿色优势在 50~95 区间逐步过渡到透明，更强且较亮的绿色背景直接设为全透明。
    alpha = np.clip((95.0 - dominance) / 45.0, 0.0, 1.0)
    alpha[(green > 145) & (dominance > 100)] = 0.0
    pixels[..., 3] = np.minimum(pixels[..., 3], (alpha * 255).astype(np.uint8))
    Image.fromarray(pixels, "RGBA").save(ROOT / output_name)


# 【批量入口】处理手臂、腿和武器；下面还依赖已存在的 sheet_body.png，本脚本不生成身体整图。
chroma_key("Codex 图像 2026年9月14日 01_31_33.png", "sheet_arms.png")
chroma_key("Codex 图像 2026年9月14日 01_31_39.png", "sheet_legs.png")
chroma_key("Codex 图像 2026年9月14日 01_32_02.png", "sheet_rapier.png")


# 【部件裁切】box 为 Pillow 的左/上/右/下像素边界；裁切后再次按 alpha 收紧透明边缘。
# name 同时决定输出文件名以及后续骨骼工具的 attachment 引用名；改名时需联动更新后续脚本。
def crop(name: str, sheet: str, box: tuple[int, int, int, int]) -> tuple[int, int]:
    image = Image.open(ROOT / sheet).convert("RGBA").crop(box)
    alpha_box = image.getchannel("A").getbbox()
    if alpha_box is None:
        raise RuntimeError(f"No visible pixels in {name}")
    image = image.crop(alpha_box)
    image.save(OUT / f"{name}.png")
    return image.size


# 【部件区域表】像素坐标针对当前素材尺寸手动选定，换素材或缩放整图后不能直接沿用。
regions = {
    "head": ("sheet_body.png", (35, 15, 340, 370)),
    "hair_front": ("sheet_body.png", (340, 0, 725, 370)),
    "hair_back": ("sheet_body.png", (710, 0, 1090, 370)),
    "feather": ("sheet_body.png", (1080, 0, 1448, 355)),
    "torso": ("sheet_body.png", (1080, 345, 1448, 735)),
    "hips": ("sheet_body.png", (15, 690, 370, 1086)),
    "collar": ("sheet_body.png", (1080, 750, 1448, 1045)),
    "upper_arm_l": ("sheet_arms.png", (45, 0, 380, 345)),
    "upper_arm_r": ("sheet_arms.png", (420, 0, 690, 345)),
    "forearm_l": ("sheet_arms.png", (820, 0, 1100, 365)),
    "forearm_r": ("sheet_arms.png", (1220, 0, 1536, 365)),
    "hand_l": ("sheet_arms.png", (45, 340, 385, 690)),
    "hand_r": ("sheet_arms.png", (430, 340, 740, 690)),
    "hand_grip": ("sheet_arms.png", (1170, 700, 1536, 1024)),
    "leg_l": ("sheet_legs.png", (120, 0, 420, 510)),
    "leg_r": ("sheet_legs.png", (650, 0, 890, 510)),
    "boot_l": ("sheet_legs.png", (1120, 70, 1480, 520)),
    "leg_l_alt": ("sheet_legs.png", (120, 500, 430, 1024)),
    "leg_r_alt": ("sheet_legs.png", (650, 500, 900, 1024)),
    "boot_r": ("sheet_legs.png", (1120, 540, 1510, 1024)),
    "rapier_long": ("sheet_rapier.png", (35, 25, 1740, 310)),
}

# 【执行与检查】遍历表内全部部件并打印实际尺寸，供检查空裁切、尺寸异常和后续 attachment 缩放。
sizes = {name: crop(name, sheet, box) for name, (sheet, box) in regions.items()}
print("\n".join(f"{name}: {size[0]}x{size[1]}" for name, size in sizes.items()))
