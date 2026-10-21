using UnityEngine;
using UnityEngine.InputSystem;

namespace HarborV2
{
    public sealed class HarborView : MonoBehaviour
    {
        public Transform target, backdrop;
        public bool Overview { get; private set; }
        private Camera view;
        private Vector3 backgroundOrigin;
        private void Awake() { view = GetComponent<Camera>(); backgroundOrigin = backdrop.position; }
        private void LateUpdate()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame) Overview = !Overview;
            if (Overview)
            {
                view.orthographicSize = Mathf.Max(12,34/view.aspect);
                transform.position = new Vector3(32,6,-10);
            }
            else if (target != null)
            {
                view.orthographicSize = Mathf.Max(7,10/view.aspect);
                float half = Mathf.Min(31,view.orthographicSize*view.aspect);
                var desired = new Vector3(Mathf.Clamp(target.position.x+1.5f,half-1,65-half),Mathf.Clamp(target.position.y+2,3,8),-10);
                transform.position = Vector3.Lerp(transform.position,desired,1-Mathf.Exp(-7*Time.unscaledDeltaTime));
            }
            backdrop.position = backgroundOrigin + new Vector3((transform.position.x-32)*.08f,0,0);
        }
    }
}
