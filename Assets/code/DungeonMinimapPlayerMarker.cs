using UnityEngine;
using UnityEngine.UI;

/// <summary>Follows the player with the dungeon map and tracks the player and boss markers.</summary>
public sealed class DungeonMinimapPlayerMarker : MonoBehaviour
{
    [SerializeField] private Camera mapCamera;
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RectTransform marker;
    [SerializeField] private RectTransform bossMarker;
    [SerializeField] private Transform player;
    [SerializeField, Range(0f, 0.1f)] private float edgePadding = 0.06f;

    private BossController boss;
    private float cameraHeight;

    private void Awake()
    {
        if (mapCamera != null) cameraHeight = mapCamera.transform.position.y;
    }

    private void Start()
    {
        if (mapCamera == null)
        {
            GameObject cameraObject = GameObject.Find("minimap");
            if (cameraObject != null) mapCamera = cameraObject.GetComponent<Camera>();
        }
        if (mapCamera != null) cameraHeight = mapCamera.transform.position.y;

        FindPlayer();
        if (bossMarker != null) bossMarker.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (mapCamera == null) return;
        if (player == null) FindPlayer();
        if (player == null) return;

        Vector3 playerPosition = player.position;
        mapCamera.transform.position = new Vector3(playerPosition.x, cameraHeight, playerPosition.z);

        // The map camera follows the player, so keep the player's HUD pin at the center.
        if (marker != null) marker.anchoredPosition = Vector2.zero;

        if (boss == null) boss = FindFirstObjectByType<BossController>();
        UpdateBossMarker();
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    private void UpdateBossMarker()
    {
        if (bossMarker == null) return;

        if (boss == null)
        {
            SetBossMarkerVisible(false);
            return;
        }

        Health bossHealth = boss.GetComponent<Health>();
        if (bossHealth != null && bossHealth.IsDead)
        {
            SetBossMarkerVisible(false);
            return;
        }

        Vector3 view = mapCamera.WorldToViewportPoint(boss.transform.position);
        if (view.z <= 0f)
        {
            SetBossMarkerVisible(false);
            return;
        }

        float x = Mathf.Clamp(view.x, edgePadding, 1f - edgePadding);
        float y = Mathf.Clamp(view.y, edgePadding, 1f - edgePadding);
        bossMarker.anchoredPosition = new Vector2(
            (x - 0.5f) * mapRect.rect.width,
            (y - 0.5f) * mapRect.rect.height);
        SetBossMarkerVisible(true);
    }

    private void SetBossMarkerVisible(bool visible)
    {
        if (bossMarker.gameObject.activeSelf != visible)
            bossMarker.gameObject.SetActive(visible);
    }
}
