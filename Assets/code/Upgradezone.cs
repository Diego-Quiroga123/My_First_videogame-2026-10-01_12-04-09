using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Zona de mejoras. Ponla en un objeto con un Collider con "Is Trigger" activado.
/// Cuando el jugador está dentro y presiona la tecla, gasta los materiales y mejora el equipo.
/// </summary>
[RequireComponent(typeof(Collider))]
public class UpgradeZone : MonoBehaviour
{
    [Header("Control")]
    [SerializeField] private KeyCode upgradeKey = KeyCode.E;

    [Header("Mensaje (opcional)")]
    [SerializeField] private TMP_Text promptText; // solo se ve mientras el jugador está en la zona

    [Header("Eventos (opcionales, para sonidos, partículas, etc.)")]
    public UnityEvent onUpgradeSuccess;
    public UnityEvent onUpgradeFailed;

    private PlayerUpgrades player;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player != null) return;

        PlayerUpgrades found = other.GetComponentInParent<PlayerUpgrades>();
        if (found == null) return;

        player = found;
        player.onItemsChanged.AddListener(OnPlayerChanged);
        player.onEquipmentLevelUp.AddListener(OnPlayerChanged);

        if (promptText != null) promptText.gameObject.SetActive(true);
        Refresh();
    }

    private void OnTriggerExit(Collider other)
    {
        if (player == null) return;
        if (other.GetComponentInParent<PlayerUpgrades>() != player) return;

        Leave();
    }

    private void OnDisable()
    {
        Leave();
    }

    private void Update()
    {
        if (player == null) return;

        if (Input.GetKeyDown(upgradeKey))
        {
            if (player.TryUpgrade()) onUpgradeSuccess?.Invoke();
            else onUpgradeFailed?.Invoke();
        }
    }

    private void Leave()
    {
        if (player != null)
        {
            player.onItemsChanged.RemoveListener(OnPlayerChanged);
            player.onEquipmentLevelUp.RemoveListener(OnPlayerChanged);
        }

        player = null;

        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void OnPlayerChanged(int _)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (promptText == null || player == null) return;

        int level = player.EquipmentLevel;

        if (player.IsMaxLevel)
        {
            promptText.text = "Equipo al máximo (nivel " + level + ")";
            return;
        }

        string reward = "+" + player.DamagePerLevel + " daño, +" + player.HealthPerLevel + " vida";
        string status = "Costo: " + player.NextLevelCost + "  |  Tienes: " + player.Materials;

        if (player.CanUpgrade)
        {
            promptText.text = "Presiona " + upgradeKey + " para mejorar (nivel " + level + " > " + (level + 1) + ")\n"
                              + reward + "\n" + status;
        }
        else
        {
            int missing = player.NextLevelCost - player.Materials;
            promptText.text = "Te faltan " + missing + " materiales para mejorar\n" + reward + "\n" + status;
        }
    }
}