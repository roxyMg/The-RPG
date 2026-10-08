using UnityEngine;

public class Enemy_Health : Entity_Health
{
    private Enemy enemy => GetComponent<Enemy>();

    public override void TakeDamage(float damage,Transform damageDealer)//如果damageDealer==Player，enemy.player=damageDealer
    {
        base.TakeDamage(damage, damageDealer);

        if(isDead)
           return;

        if(damageDealer.GetComponent<Player>() != null)
           enemy.TryEnterBattleState(damageDealer);
           
        
    }

}
