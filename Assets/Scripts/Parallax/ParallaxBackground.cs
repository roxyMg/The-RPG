using UnityEngine;

// 视差背景协调器：根据相机每帧的水平位移移动各层，并在离屏后循环图片。
public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    // 保存上一物理帧的相机 x 坐标，用差值驱动视差，而不是直接使用世界坐标。
    private float lastCameraPositionX; 
    private float cameraHalfWidth;

   // 数组顺序不影响计算；每层各自持有背景 Transform 和视差倍率。
   [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        mainCamera = Camera.main;
        // 正交相机半宽 = orthographicSize（半高）× 宽高比。
        // 该值只在 Awake 计算，运行时改变分辨率或相机尺寸时不会自动刷新。
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        CalculateImageLengths();
    }

    private void FixedUpdate()
    {
        float currentCameraPositionX = mainCamera.transform.position.x;
        // lastCameraPositionX 初始值为 0，因此首帧位移以世界原点为基准。
        float distanceToMove = currentCameraPositionX - lastCameraPositionX;
        lastCameraPositionX = currentCameraPositionX;

        float cameraLeftEdge = currentCameraPositionX - cameraHalfWidth;
        float cameraRightEdge = currentCameraPositionX + cameraHalfWidth;

        foreach (ParallaxLayer layer in backgroundLayers)
        {
            // 先移动，再用移动后的边界判断是否需要把图层循环到另一侧。
            layer.Move(distanceToMove);
            layer.loopBackground(cameraLeftEdge, cameraRightEdge);
        }
    }

    private void CalculateImageLengths()
    {
        // 缓存宽度，避免在每个 FixedUpdate 中重复查询 SpriteRenderer.bounds。
        foreach (ParallaxLayer layer in backgroundLayers)
            layer.CalculateImageWidth();
    }
}
