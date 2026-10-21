using UnityEngine;

namespace CombatTest
{
    // Fresh actors on every retry; dispose the generated input wrapper after OnDisable.
    public sealed class TestPlayerInputLifetime : MonoBehaviour
    {
        private Player player;
        private void Awake() => player = GetComponent<Player>();
        private void OnDestroy() => player?.input?.Dispose();
    }
}
