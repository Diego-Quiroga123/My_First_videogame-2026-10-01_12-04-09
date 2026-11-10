using UnityEngine;
using UnityEngine.UI;

/// <summary>Keeps a clear HUD marker aligned with the player's world position on the dungeon map.</summary>
public sealed class DungeonMinimapPlayerMarker : MonoBehaviour
{
    [SerializeField] private Camera mapCamera;
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RectTransform marker;
    [SerializeField] private Transform player;
    [SerializeField, Range(0f, 0.1f)] private float edgePadding = 0.06f;

    private void Start()
    {
        if (mapCamera == null)
        {
            GameObject cameraObject = GameObject.Find("minimap");
            if (cameraObject != null) mapCamera = cameraObject.GetComponent<Camera>();
        }
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }
    }

    private void LateUpdate()
    {
        if (mapCamera == null || mapRect == null || marker == null || player == null) return;

        Vector3 view = mapCamera.WorldToViewportPoint(player.position);
        if (view.z <= 0f) return;

        float paddingX = edgePadding;
        float paddingY = edgePadding;
        float x = Mathf.Clamp(view.x, paddingX, 1f - paddingX);
        float y = Mathf.Clamp(view.y, paddingY, 1f - paddingY);
        marker.anchoredPosition = new Vector2((x - 0.5f) * mapRect.rect.width, (y - 0.5f) * mapRect.rect.height);
    }
}
