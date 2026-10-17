using UnityEngine;

public class Enemy_Health : Entity_Health
{
    private Enemy enemy => GetComponent<Enemy>();
    private bool receivingExecutionDamage;

    // 标记本次伤害来源；仅此调用链中的击杀享受处决回弹。
    public void TakeExecutionDamage(float damage, Transform damageDealer)
    {
        bool previous = receivingExecutionDamage;
        receivingExecutionDamage = true;
        try
        {
            TakeDamage(damage, damageDealer, 0);
        }
        finally
        {
            receivingExecutionDamage = previous;
        }
    }

    public override void TakeDamage(float damage, Transform damageDealer, int impactLevel)//如果damageDealer==Player，enemy.player=damageDealer
    {
        // 仅在本次伤害造成存活到死亡的转换时奖励，重复命中尸体不补弹。
        if (isDead)
            return;
        bool isExecutionKill = receivingExecutionDamage;
        base.TakeDamage(damage, damageDealer, impactLevel);

        if (isDead)
        {
            if (damageDealer != null && damageDealer.GetComponent<Player>() != null)
                damageDealer.GetComponent<Player_LethalAmmo>()?.RecordKill(isExecutionKill);
            return;
        }

        if(damageDealer != null && damageDealer.GetComponent<Player>() != null)
           enemy.TryEnterBattleState(damageDealer);
           
        
    }

}
