using UnityEngine;

namespace HarborV2
{
    public sealed class HarborPoint : MonoBehaviour
    {
        public enum Kind { Permit, Checkpoint, Winch, Boat, Relic }
        public Kind kind;
        public string prompt;
        public float radius = 2.2f;
        public SpriteRenderer visual;
        public bool used;
        public void Refresh()
        {
            if (visual != null) visual.color = used ? new Color(.6f,.8f,.83f) : Color.white;
        }
    }
}
