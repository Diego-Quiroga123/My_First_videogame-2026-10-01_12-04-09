using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Posición")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -7f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Zoom de la escena Base")]
    [SerializeField, Range(0.25f, 1f)] private float closestZoom = 0.55f;
    [SerializeField, Min(0.01f)] private float zoomSensitivity = 0.8f;
    [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.12f;

    private Vector3 currentVelocity;
    private float currentZoom = 1f;
    private float targetZoom = 1f;
    private float zoomVelocity;

    private void LateUpdate()
    {
        if (target == null) return;

        if (SceneManager.GetActiveScene().name == "Base")
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            targetZoom = Mathf.Clamp(targetZoom - wheel * zoomSensitivity, closestZoom, 1f);
            currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);
        }
        else
        {
            currentZoom = 1f;
            targetZoom = 1f;
        }

        Vector3 desiredPosition = target.position + offset * currentZoom;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }
}
