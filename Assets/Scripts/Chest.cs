using UnityEngine;

public class Chest : MonoBehaviour , IDamgable
{
    private Rigidbody2D rb => GetComponentInChildren<Rigidbody2D>();
    private Animator anim => GetComponentInChildren<Animator>();
    private Entity_VFX vFX => GetComponent<Entity_VFX>();

    [Header("箱子打开细节")]
    [SerializeField] private Vector2 knockback;
    public void TakeDamage(float damage, Transform damageDealer)
    {
       vFX.PlayOnDamageVFX();
       anim.SetBool("ChestOpen", true);
       rb.linearVelocity = knockback;
       rb.angularVelocity = Random.Range(-200f,200f);

       //还有掉落物没设置
    }
}
