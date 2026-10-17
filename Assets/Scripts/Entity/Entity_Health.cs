using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Entity_HitReaction))]
public class Entity_Health : MonoBehaviour, IDamgable
{
    private Slider heathBar;
    private Entity_VFX entityVFX;
    private Entity entity;
    private Entity_HitReaction hitReaction;
    private Entity_Invulnerability invulnerability;
    [SerializeField] protected float maxHP = 100;
    [SerializeField] protected float currentHP;
    [SerializeField] protected bool isDead;

    // 只读 HUD 数据，不改变原血条更新与扣血逻辑。
    public bool IsDead => isDead;
    public float CurrentHealth => currentHP;
    public float NormalizedHealth => maxHP > 0 ? Mathf.Clamp01(currentHP / maxHP) : 0;

    protected virtual void Awake()
    {
        entity = GetComponent<Entity>();
        hitReaction = GetComponent<Entity_HitReaction>();
        invulnerability = GetComponent<Entity_Invulnerability>();
        entityVFX = GetComponent<Entity_VFX>();
        heathBar = GetComponentInChildren<Slider>();
    
        currentHP = maxHP;
        UpdateHealthBar();
    }

    
    // 旧调用默认使用等级 0；通过虚方法分派保留敌人受伤逻辑。
    public virtual void TakeDamage(float damage,Transform damageDealer)
    {
        TakeDamage(damage, damageDealer, 0);
    }

    public virtual void TakeDamage(float damage, Transform damageDealer, int impactLevel)
    {
        if (isDead || (invulnerability != null && invulnerability.IsInvulnerable))
           return;

        entityVFX?.PlayOnDamageVFX();
        hitReaction.ReceiveHit(damageDealer, impactLevel);
        ReduceHP(damage);
    }
    protected void ReduceHP(float damage)
    {
        currentHP -= damage;
        UpdateHealthBar();

        if(currentHP <= 0)
           Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        entity.EntityDeath();
    }

    private void UpdateHealthBar()
    {
        if(heathBar == null)
           return;

        heathBar.value = currentHP / maxHP;
    }
}
