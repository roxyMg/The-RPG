using UnityEngine;

// 允许 ParallaxLayer 作为普通可序列化对象直接显示在 MonoBehaviour 的 Inspector 数组中。
[System.Serializable]
public class ParallaxLayer
{
   // background 需要带 SpriteRenderer；图片宽度用于判断整张图是否完全离开相机。
   [SerializeField] private Transform background;
   // 0 表示不跟随相机移动，1 表示与相机等量移动，中间值形成远近层次。
   [SerializeField] private float parallaxMultiplier;

   private float imageFullWidth;
   private float imageHalfWidth;

   public void CalculateImageWidth()
    {
        // bounds.size.x 使用世界空间宽度，因此会包含 Transform 缩放。
        imageFullWidth = background.GetComponent<SpriteRenderer>().bounds.size.x;
        imageHalfWidth = imageFullWidth / 2;
    }

   public void Move(float distanceToMove)
    {
        // 只处理水平视差；垂直相机移动不会影响背景层。
        background.position += Vector3.right * (distanceToMove * parallaxMultiplier); //new Vector3(distanceToMove * parallaxMultiplier, 0);
    }

    public void loopBackground(float cameraLeftEdge, float cameraRightEdge)
    {
        // 只有整张图片越过相机边缘后才搬到另一侧，避免仍可见时产生跳变。
        float imageLeftEdge = background.position.x - imageHalfWidth;
        float imageRightEdge = background.position.x + imageHalfWidth;

        // 每次只移动一个图片宽度，要求素材本身可无缝平铺且宽度足以覆盖视野。
        if (imageRightEdge < cameraLeftEdge)
            background.position += Vector3.right * imageFullWidth;
        else if (imageLeftEdge > cameraRightEdge)
            background.position += Vector3.left * imageFullWidth;
    }
}
