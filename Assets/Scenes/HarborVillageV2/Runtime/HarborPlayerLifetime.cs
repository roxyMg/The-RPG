using UnityEngine;

namespace HarborV2
{
    // Resource ownership only. No movement, physics or combat tuning lives here.
    public sealed class HarborPlayerLifetime : MonoBehaviour
    {
        private Player player;
        private void Awake() => player = GetComponent<Player>();
        private void OnDestroy() => player?.input?.Dispose();
    }
}
