using UnityEngine;

namespace HarborV2
{
    public sealed class HarborCrate : MonoBehaviour, IDamgable
    {
        public float Health { get; private set; } = 30;
        public int Hits { get; private set; }
        public void TakeDamage(float damage, Transform dealer) => TakeDamage(damage, dealer, 0);
        public void TakeDamage(float damage, Transform dealer, int impact)
        {
            if (Health <= 0) return;
            Health = Mathf.Max(0,Health-damage); Hits++;
            GetComponent<SpriteRenderer>().color = Health <= 0 ? new Color(.35f,.4f,.45f,.35f) : new Color(1,.72f,.6f);
            GetComponent<Collider2D>().enabled = Health > 0;
        }
        public void ResetCrate()
        {
            Health = 30; Hits = 0; GetComponent<SpriteRenderer>().color = Color.white;
            GetComponent<Collider2D>().enabled = true;
        }
    }
}
