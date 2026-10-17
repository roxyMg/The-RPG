using UnityEngine;
using UnityEngine.Serialization;

// 受击组件：按冲击等级选择位移参数，执行仍交给 Entity。
public class Entity_HitReaction : MonoBehaviour
{
    private Entity entity;

    [Header("普通击退：冲击 1")]
    [SerializeField] private Vector2 onDamageKnockback = new Vector2(1.5f, 2.5f);
    [SerializeField] private float knockbackDuration = .2f;

    [Header("击飞：冲击 2")]
    [FormerlySerializedAs("onHeavyDamageKnockback")]
    [SerializeField] private Vector2 launchVelocity = new Vector2(7f, 7f);
    [FormerlySerializedAs("heavyKnockDuration")]
    [SerializeField] private float launchDuration = .5f;

    private void Awake()
    {
        entity = GetComponent<Entity>();
    }

    public void ReceiveHit(Transform damageDealer, int impactLevel)
    {
        if (impactLevel != 1 && impactLevel != 2)
            return;
        int direction = transform.position.x >= damageDealer.position.x ? 1 : -1;
        ReceiveImpact(impactLevel, direction);
    }

    // 失衡等状态复用同一套冲击配置，方向由调用方提供。
    public void ReceiveImpact(int impactLevel, int direction)
    {
        Vector2 velocity;
        float duration;
        switch (impactLevel)
        {
            case 0:
                return; // 无受击位移；伤害和闪白由其他组件处理。
            case 1:
                velocity = onDamageKnockback;
                duration = knockbackDuration;
                break;
            case 2:
                velocity = launchVelocity;
                duration = launchDuration;
                break;
            case 3:
                return; // 预留倒地，待专用状态与动画接入。
            default:
                return; // 未知等级不触发位移。
        }

        velocity.x = Mathf.Abs(velocity.x) * direction;
        entity.ReceiveKnockback(velocity, duration);
    }
}
