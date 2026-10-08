using UnityEngine;

public interface IDamgable//可受攻击的接口
{
    public void TakeDamage(float damage,Transform damageDealer);
}
