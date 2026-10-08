# 宝箱开启动画

- 原图：treasure_chest_open_8frames.png，1774 × 887，RGBA 透明背景。
- 8 帧，4 列 × 2 行，先上排后下排，每排从左到右。
- 已配置 Unity Multiple Sprite 切片，每帧 443 × 443。原图尺寸为奇数，切片跳过透明分隔像素；不要直接使用等分 Grid Cell Count 覆盖切片。
- 统一 Pivot (0.5, 0.08)，256 Pixels Per Unit，可按场景比例调整。
- 建议 10 FPS，关闭循环；第 0 帧为关闭，第 7 帧为打开。反向播放可用于关闭。
- 在 Unity 展开 PNG，选择 Open_00 至 Open_07，拖入场景或 Animation 窗口即可创建动画；将 Samples 设为 10，并取消 Loop Time。
- 图像已检查透明通道及八个切片范围；尚未在 Unity 编辑器运行验证。AI 绘制帧间纹理和装饰有少量变化。

生成方式：内置 image_gen。以下为实际生成提示词（请求尺寸与工具返回尺寸不同，上述参数以返回文件为准）。

Use case: stylized-concept. Asset type: production 2D game sprite sheet, 8 frame treasure chest opening animation for Unity. Input image is a DESIGN REFERENCE ONLY: reproduce its treasure chest identity, dark weathered wood, aged brass riveted bands, gothic pointed arches, purple center cloth with fleur-de-lis and cross, side ring handle, ornate lock, gold religious relics inside. Generate ONE transparent PNG spritesheet, 2048x1024, exactly 4 columns x 2 rows of equally sized 512x512 cells, no spacing between cells. Each sprite entirely inside its cell with generous transparent padding. Reading order left to right top then bottom: 8 progressive frames lid angles 0, 8, 20, 36, 54, 72, 88, 100 degrees. Frame 1 fully CLOSED, frame 8 fully OPEN with gold reliquary and cup visible. SAME chest, SAME size, SAME camera in ALL eight frames, front and left side visible as in reference. Lower chest body is completely motionless and identical geometry in every frame, bottom ground contact at local y=464, center x=256, chest overall width about 416 pixels. Lid hinges at the back, physically rotates upward; ornamented latch follows lid. The body must NOT grow or shrink, camera must not rotate. Lid silhouette and dark purple lining visible when opened. Gold light subtly emerges from the gap beginning frame 3, warm interior illumination increases, no large external halo. Hand-painted anime-inspired dark fantasy game asset, crisp dark outlines, simplified readable detail, high quality aged materials faithful to reference. Actual transparent background alpha=0 outside objects; NO checkerboard drawn in image, NO white/gray backdrop, NO floor, NO cast floor shadows, NO scenery, NO captions, NO numbers, NO grid lines, NO headings, NO extra objects. Keep all parts separated from cell boundaries. The sheet must be directly sliceable into eight uniform frames.

