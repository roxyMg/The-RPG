using UnityEngine;

public class Enemy_DeadState : EnemyState
{
    private SpriteRenderer[] sprites;

    public Enemy_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        sprites = enemy.GetComponentsInChildren<SpriteRenderer>();
    }

    public override void Enter()
    {
        anim.enabled = false;

        rb.gravityScale = 12;
        rb.velocity = new Vector2(rb.velocity.x * enemy.deathHorizontalMultiplier, enemy.deathLaunchForce);

        stateMachine.SwitchOffStateMachine();

        // 设置消散时间
        stateTimer = enemy.deathFadeDuration;

        Debug.LogWarning("死了喵");
    }

    public override void Update()
    {
        base.Update();

        WhenDead();

    }

    private void WhenDead()//死亡时消散
    {
        float alpha =Mathf.Clamp01(stateTimer / enemy.deathFadeDuration);

        SetAlpha(alpha);
    
        if (stateTimer <= 0)
        {
            Object.Destroy(enemy.gameObject);
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer sr in sprites)
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }
    }
}
