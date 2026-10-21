using System.Collections.Generic;
using UnityEngine;

namespace CombatTest
{
    // Only the test-room prefab uses this subclass. The ordinary skeleton is untouched.
    public sealed class Enemy_TestElite : Enemy_Skeleton
    {
        public enum Move { Cleave, Rush, Slam }
        public enum Phase { Waiting, Windup, Active, Recovery, Stunned, Dead }
        public Move CurrentMove { get; private set; }
        public Phase CurrentPhase { get; private set; }
        public int AttacksStarted { get; private set; }
        public int HitsLanded { get; private set; }
        public float PhaseProgress { get; private set; }
        public Player Target { get; private set; }
        public Entity_Health Health { get; private set; }
        public float arenaHalfWidth = 14;
        public Sprite markerSprite;
        private SpriteRenderer warning;
        private readonly HashSet<Entity_Health> hitThisAttack = new HashSet<Entity_Health>();
        private float timer, readyAt;
        private int nextMove;
        private Vector2 lockedOrigin;
        private int lockedDirection;
        private static readonly float[] Windups = { .7f, .9f, 1.15f };
        private static readonly float[] Actives = { .18f, .42f, .22f };
        private static readonly float[] Recoveries = { .8f, .95f, 1.2f };
        private static readonly float[] Damage = { 12, 16, 22 };
        public float Windup => Windups[(int)CurrentMove];
        public float ActiveTime => Actives[(int)CurrentMove];
        public float Recovery => Recoveries[(int)CurrentMove];
        public string MoveLabel => CurrentMove == Move.Cleave ? "横斩 / CLEAVE" : CurrentMove == Move.Rush ? "突进 / RUSH" : "重砸 / SLAM";

        protected override void Awake()
        {
            base.Awake();
            Health = GetComponent<Entity_Health>();
            BattleState = new EliteBattle(this, stateMachine);
            AttackState = new EliteAttack(this, stateMachine);
            var marker = new GameObject("Attack footprint (visual only)");
            marker.transform.SetParent(transform, false);
            warning = marker.AddComponent<SpriteRenderer>();
            warning.sprite = markerSprite;
            warning.sortingLayerName = "Default";
            warning.sortingOrder = 10;
            warning.enabled = false;
        }

        protected override void Start()
        {
            base.Start();
            Target = FindFirstObjectByType<Player>();
            readyAt = Time.time + 2;
            if (Target != null) TryEnterBattleState(Target.transform);
        }

        protected override void Update()
        {
            if (Health.IsDead)
            {
                CurrentPhase = Phase.Dead;
                CloseWarning();
                base.Update();
                return;
            }
            if (Target == null || Target.GetComponent<Entity_Health>().IsDead)
            {
                CloseWarning();
                SetVelocity(0, rb.linearVelocity.y);
                return;
            }
            base.Update();
            if (IsStunned || IsExecutionLocked)
            {
                CurrentPhase = Phase.Stunned;
                CloseWarning();
                anim.speed = 1;
            }
            else if (stateMachine.currentState != AttackState && stateMachine.currentState != BattleState)
                TryEnterBattleState(Target.transform);
        }

        private void Chase()
        {
            CurrentPhase = Phase.Waiting;
            PhaseProgress = 0;
            if (isKnocked || Target == null) return;
            float dx = Target.transform.position.x - transform.position.x;
            HandFlip(dx);
            float range = nextMove == 1 ? 6 : nextMove == 2 ? 3.3f : 2.5f;
            if (Time.time >= readyAt && groundDetected && Mathf.Abs(dx) <= range && Mathf.Abs(Target.transform.position.y - transform.position.y) < 2)
            {
                CurrentMove = (Move)nextMove;
                nextMove = (nextMove + 1) % 3;
                stateMachine.ChangeState(AttackState);
                return;
            }
            SetVelocity(Mathf.Abs(dx) > 2 ? Mathf.Sign(dx) * battleMoveSpeed : 0, rb.linearVelocity.y);
        }

        private void BeginAttack()
        {
            timer = 0;
            AttacksStarted++;
            lockedDirection = facingDirection;
            lockedOrigin = transform.position;
            hitThisAttack.Clear();
            SetVelocity(0, rb.linearVelocity.y);
            CurrentPhase = Phase.Windup;
            // The test override clip contains no old AttackTrigger/counter events.
            anim.Play("SkeletonAttack", 0, 0);
            anim.speed = 0;
            EnableCounterWindow(CurrentMove != Move.Slam);
            GetComponent<Enemy_VFX>().EnableAttackAlert(CurrentMove != Move.Slam);
        }

        private void TickAttack()
        {
            timer += Time.deltaTime;
            if (timer < Windup)
            {
                CurrentPhase = Phase.Windup;
                PhaseProgress = timer / Windup;
                anim.Play("SkeletonAttack", 0, Mathf.Lerp(0, .33f, PhaseProgress));
                ShowWarning(false);
            }
            else if (timer < Windup + ActiveTime)
            {
                CurrentPhase = Phase.Active;
                PhaseProgress = (timer - Windup) / ActiveTime;
                EnableCounterWindow(false);
                GetComponent<Enemy_VFX>().EnableAttackAlert(false);
                anim.Play("SkeletonAttack", 0, Mathf.Lerp(.34f, .58f, PhaseProgress));
                if (CurrentMove == Move.Rush && !isKnocked)
                    SetVelocity(Mathf.Abs(transform.position.x) < arenaHalfWidth - .9f ? lockedDirection * 10 : 0, rb.linearVelocity.y);
                ShowWarning(true);
                HitTargets();
            }
            else
            {
                CloseWarning();
                CurrentPhase = Phase.Recovery;
                PhaseProgress = Mathf.Clamp01((timer - Windup - ActiveTime) / Recovery);
                SetVelocity(0, rb.linearVelocity.y);
                anim.Play("SkeletonAttack", 0, Mathf.Lerp(.6f, .99f, PhaseProgress));
                if (timer >= Windup + ActiveTime + Recovery)
                    stateMachine.ChangeState(BattleState);
            }
        }

        // World-space footprint is locked when the tell begins. Rush hitbox follows its body.
        public Bounds HitBounds(bool preview)
        {
            Vector2 origin = CurrentMove == Move.Rush && !preview ? (Vector2)transform.position : lockedOrigin;
            float width = CurrentMove == Move.Cleave ? 2.8f : CurrentMove == Move.Rush ? (preview ? 6 : 2.2f) : 6.8f;
            float height = CurrentMove == Move.Slam ? .8f : 2.4f;
            float offset = CurrentMove == Move.Slam ? 0 : width * .5f + .25f;
            return new Bounds(new Vector3(origin.x + lockedDirection * offset, height * .5f, 0), new Vector3(width, height, 1));
        }

        private void HitTargets()
        {
            var box = HitBounds(false);
            foreach (var hit in Physics2D.OverlapBoxAll(box.center, box.size, 0, 1 << LayerMask.NameToLayer("Player")))
            {
                var health = hit.GetComponentInParent<Entity_Health>();
                if (health == null || health.IsDead || !hitThisAttack.Add(health)) continue;
                float before = health.CurrentHealth;
                health.TakeDamage(Damage[(int)CurrentMove], transform, CurrentMove == Move.Slam ? 2 : 1);
                if (health.CurrentHealth < before) HitsLanded++;
            }
        }

        private void ShowWarning(bool active)
        {
            var box = HitBounds(!active);
            warning.enabled = true;
            warning.transform.position = new Vector3(box.center.x, .08f, 0);
            warning.transform.rotation = Quaternion.identity;
            warning.transform.localScale = new Vector3(box.size.x / transform.lossyScale.x, .12f / transform.lossyScale.y, 1);
            warning.color = CurrentMove == Move.Slam ? new Color(1, .24f, .22f, active ? .95f : .55f) : new Color(1, .76f, .25f, active ? .95f : .55f);
        }

        private void CloseWarning()
        {
            if (warning != null) warning.enabled = false;
            EnableCounterWindow(false);
            GetComponent<Enemy_VFX>().EnableAttackAlert(false);
        }

        private void EndAttack()
        {
            CloseWarning();
            anim.speed = 1;
            SetVelocity(0, rb.linearVelocity.y);
            readyAt = Time.time + .35f;
        }

        private sealed class EliteBattle : Enemy_BattleState
        {
            private readonly Enemy_TestElite elite;
            public EliteBattle(Enemy_TestElite e, StateMachine sm) : base(e, sm, "Battle") { elite = e; }
            public override void Enter() { anim.SetBool("Battle", true); anim.speed = 1; }
            public override void Update() { UpdateAnimationParameters(); elite.Chase(); }
        }

        private sealed class EliteAttack : Enemy_AttackState
        {
            private readonly Enemy_TestElite elite;
            public EliteAttack(Enemy_TestElite e, StateMachine sm) : base(e, sm, "Attack") { elite = e; }
            public override void Enter() { base.Enter(); elite.BeginAttack(); }
            public override void Update() { elite.TickAttack(); }
            public override void Exit() { base.Exit(); elite.EndAttack(); }
        }
    }
}
