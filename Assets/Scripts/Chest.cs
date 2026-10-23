using UnityEngine;

public class Chest : MonoBehaviour , IDamgable
{
    private Rigidbody2D rb => GetComponentInChildren<Rigidbody2D>();
    private Animator anim => GetComponentInChildren<Animator>();
    private Entity_VFX vFX => GetComponent<Entity_VFX>();

    [Header("箱子打开细节")]
    [SerializeField] private Vector2 knockback;
    // 宝箱接受新伤害入口，继续使用原开箱表现。
    public void TakeDamage(float damage, Transform damageDealer, int impactLevel)
    {
        TakeDamage(damage, damageDealer);
    }

    public void TakeDamage(float damage, Transform damageDealer)
    {
       vFX.PlayOnDamageVFX();
       anim.SetBool("ChestOpen", true);
       rb.velocity = knockback;
       rb.angularVelocity = Random.Range(-200f,200f);

       //还有掉落物没设置
    }
}
