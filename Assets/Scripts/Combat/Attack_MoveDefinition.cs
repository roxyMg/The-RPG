using UnityEngine;

// 招式节点：复用攻击属性，配置动画下标和两种输入的后续招式。
[CreateAssetMenu(fileName = "Attack_MoveDefinition", menuName = "The RPG/Combat/Attack Move Definition")]
public class Attack_MoveDefinition : ScriptableObject
{
    [SerializeField] private Attack_Definition attackDefinition;
    [SerializeField, Min(0)] private int animationIndex;
    [Header("派生：空引用表示该输入不接招")]
    [SerializeField] private Attack_MoveDefinition normalFollowUp;
    [SerializeField] private Attack_MoveDefinition alternativeFollowUp;
    [Header("派生输入窗口：从进入本招式起计时，单位秒")]
    [Tooltip("关闭时允许整段攻击缓存派生；已有未配置招式保持原行为。")]
    [SerializeField] private bool branchWindowEnabled;
    [SerializeField, Min(0)] private float branchWindowStart = 0.2f;
    [SerializeField, Min(0)] private float branchWindowEnd = 0.6f;
    public bool BranchWindowEnabled => branchWindowEnabled;
    public float BranchWindowStart => Mathf.Max(0, branchWindowStart);
    // 防止运行时读到负数或倒置的时间区间。
    public float BranchWindowEnd => Mathf.Max(BranchWindowStart, branchWindowEnd);
    public Attack_Definition AttackDefinition => attackDefinition;
    public int AnimationIndex => animationIndex;
    public Attack_MoveDefinition NormalFollowUp => normalFollowUp;
    public Attack_MoveDefinition AlternativeFollowUp => alternativeFollowUp;
}
