using System.Collections;
using Unity.InferenceEngine;
using UnityEngine;

public class Entity_VFX : MonoBehaviour
{

  private SpriteRenderer sr;

  [Header("攻击VFX")]
    [SerializeField] private Material onDamageMaterial;
    [SerializeField] private float onDamageVFXDuration = .2f;
    private Material originalMaterial;
    private Coroutine onDamageVFXCoroutine;

  [Header("受击VFX")]
    [SerializeField] private Color hitvfxColor = Color.white;    
    [SerializeField] private GameObject hitvfx;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
    }

    public void CreateOnHitVFX(Transform target)
   {
      GameObject vfx = Instantiate(hitvfx, target.position, Quaternion.identity);
      vfx.GetComponentInChildren<SpriteRenderer>().color = hitvfxColor;
   }
    public void PlayOnDamageVFX()
    {
      if(onDamageVFXCoroutine != null)
         StopCoroutine(onDamageVFXCoroutine);

        onDamageVFXCoroutine = StartCoroutine(OnDamageVFX());
    }

    private IEnumerator OnDamageVFX()
    {
      sr.material = onDamageMaterial;

      yield return new WaitForSeconds(onDamageVFXDuration);
      sr.material = originalMaterial;
    }
      



}
