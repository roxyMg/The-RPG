using UnityEngine;

/// <summary>
/// Lightweight camera-relative parallax for the four supplied background layers.
/// A factor of 0 means camera-locked; 1 means world-locked.
/// </summary>
public sealed class HarborParallaxLayer : MonoBehaviour
{
    [Range(0f, 1.5f)] public float horizontalFactor = 0.15f;
    [Range(0f, 1.5f)] public float verticalFactor = 0.05f;
    public Transform targetCamera;

    private Vector3 originPosition;
    private Vector3 originCameraPosition;

    private void OnEnable()
    {
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }

        originPosition = transform.position;
        if (targetCamera != null)
        {
            originCameraPosition = targetCamera.position;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 delta = targetCamera.position - originCameraPosition;
        transform.position = originPosition + new Vector3(
            delta.x * horizontalFactor,
            delta.y * verticalFactor,
            0f
        );
    }
}
