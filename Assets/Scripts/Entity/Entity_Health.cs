using UnityEngine;
using UnityEngine.UI;

public class Entity_Health : MonoBehaviour, IDamgable
{
    private Slider heathBar;
    private Entity_VFX entityVFX;
    private Entity entity;
    [SerializeField] protected float maxHP = 100;
    [SerializeField] protected float currentHP;
    [SerializeField] protected bool isDead;
    [Header("受击后退")]
    [SerializeField] protected Vector2 onDamageKnockback = new Vector2(1.5f, 2.5f);
    [SerializeField] protected Vector2 onHeavyDamageKnockback = new Vector2(7f, 7f);
    [SerializeField] protected float knockbackDuration = .2f;
    [SerializeField] private float heavyKnockDuration = .5f;
    [SerializeField] private float heavyDamageThreshold = .3f;//失去一定百分比伤害才严重反击
    
    

    protected virtual void Awake()
    {
        entity = GetComponent<Entity>();
        entityVFX = GetComponent<Entity_VFX>();
        heathBar = GetComponentInChildren<Slider>();
    
        currentHP = maxHP;
        UpdateHealthBar();
    }

    
    public virtual void TakeDamage(float damage,Transform damageDealer)
    {
        if(isDead)
           return;

        float duration = CalculateDuration(damage);
        Vector2 knockback = CalculateDamageKnockback(damage, damageDealer);

        entityVFX?.PlayOnDamageVFX();
        entity.ReceiveKnockback(knockback, duration);
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
    protected virtual Vector2 CalculateDamageKnockback(float damage, Transform damageDealer)
    {
        bool heavyDamage = IsHeavyDamage(damage);
        int direction = transform.position.x >= damageDealer.position.x ? 1 : -1;
        
        Vector2 knockback = heavyDamage ? onHeavyDamageKnockback : onDamageKnockback;
        knockback.x = Mathf.Abs(knockback.x) * direction;

        return knockback;
    }

    private float CalculateDuration(float damage)
    {
        return IsHeavyDamage(damage) ? heavyKnockDuration : knockbackDuration;
    }

    private bool IsHeavyDamage(float damage) => damage / maxHP >= heavyDamageThreshold;

  
}
