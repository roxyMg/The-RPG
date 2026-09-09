using UnityEngine;

// 动画事件桥接器：挂在带 Animator 的子对象上，把动画片段事件转发给父级 Entity。
public class Entity_AnimationTriggers : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        // 依赖层级中存在 Entity；动画子对象本身不需要持有状态机。
        entity = GetComponentInParent<Entity>();
    }

    // Animation Event 的函数名需要配置为 CurrentStateTrigger。
    private void CurrentStateTrigger()
    {
        entity.CallAnimationTrigger();
    }

}
