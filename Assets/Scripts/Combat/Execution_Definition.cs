using UnityEngine;

[CreateAssetMenu(fileName = "Execution_Definition", menuName = "The RPG/Combat/Execution Definition")]
public class Execution_Definition : ScriptableObject
{
    [Header("基础处决伤害倍率")]
    [SerializeField, Min(0)] private float damageMultiplier = 5;
    [Header("动作时序")]
    [SerializeField, Min(.01f)] private float duration = .8f;
    [SerializeField, Min(0)] private float hitTime = .4f;
    [Header("处决动画：参数必须存在于 Animator")]
    [SerializeField] private string animationBoolName = "Execution";
    public float DamageMultiplier => Mathf.Max(0, damageMultiplier);
    public float Duration => Mathf.Max(.01f, duration);
    public float HitTime => Mathf.Clamp(hitTime, 0, Duration);
    public string AnimationBoolName => animationBoolName;
    private void OnValidate()
    {
        damageMultiplier = Mathf.Max(0, damageMultiplier);
        duration = Mathf.Max(.01f, duration);
        hitTime = Mathf.Clamp(hitTime, 0, duration);
    }
}
