using UnityEngine;//销毁游戏对象
public class VFX_AutoController : MonoBehaviour
{
    [SerializeField] private bool autoDestroy = true;
    [SerializeField] private float destroyDelay = 1;
    [Space]
    [SerializeField] private bool randomOffset = true;

    [Header("随机位置")]    
    [SerializeField] private float xMinoffset = -.3f;
    [SerializeField] private float xMaxoffset = .3f;
    [Space]
    [SerializeField] private float yMinoffset = -.3f;
    [SerializeField] private float yMaxoffset = .3f;

    private void Start()
    {
        ApplyRandomOffset();

        if(autoDestroy)
           Destroy(gameObject, destroyDelay);
    }

    private void ApplyRandomOffset()
    {
        if(randomOffset == false)
           return;
        
        float xoffset = Random.Range(xMinoffset,xMaxoffset);
        float yoffset = Random.Range(yMinoffset,yMaxoffset);

        transform.position = transform.position + new Vector3(xoffset,yoffset);
    }
}
