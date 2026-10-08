using UnityEngine;

public class Player_Combat : Entity_Combat
{
    [Header("反击属性")]
    [SerializeField] private float counterDuration;
    public bool CounterAttackPerformed()
    {
        bool hasCounteredSomebody = false;

        foreach(var target in GetDetectedColliders())//把当前攻击判定范围里的所有 Collider 都遍历一遍
        {
            ICounterable counterable = target.GetComponent<ICounterable>();//检测攻击范围内的目标是否有ICounterable
            
            if(counterable == null)
               continue;
               
            if(counterable.CanBeCountered)
            {
                counterable.HandleCounter();
                hasCounteredSomebody = true;
            }
            
        }
        return hasCounteredSomebody;
    }

    public float GetCounterDuration() => counterDuration;
}
