using UnityEngine;

[CreateAssetMenu( fileName = "Attack_Definition", menuName = "The RPG/Combat/Attack Definition")]
public class Attack_Definition : ScriptableObject
{
    [Header("基础伤害")]
    [SerializeField, Min(0f)]
    private float baseDamage = 10f;

    [Header("命中判定半径")]
    [SerializeField, Min(0.01f)]
    private float hitRadius = 1f;

    [Header("冲击等级")]
    [Tooltip("0:无受击位移;1:普通击退;2:击飞;3:预留倒地（暂不触发位移）。")]
    [SerializeField, Range(0, 3)]
    private int impactLevel = 0;

    [Header("近战装填：每名有效受伤的敌人")]
    [SerializeField, Min(0)] private float reloadProgressOnHit;
    public float ReloadProgressOnHit => Mathf.Max(0, reloadProgressOnHit);

    public float BaseDamage => baseDamage;
    public float HitRadius => hitRadius;
    public int ImpactLevel => impactLevel;
}
