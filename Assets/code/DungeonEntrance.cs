using TMPro;
using UnityEngine;

/// <summary>
/// Entrada al calabozo, para la escena Base. Ponlo en un objeto con un Collider con "Is Trigger".
/// El jugador se para en la zona y presiona la tecla para empezar el piso.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DungeonEntrance : MonoBehaviour
{
    [SerializeField] private KeyCode enterKey = KeyCode.E;

    [Header("Mensaje (opcional)")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string message = "Presiona E para entrar al calabozo";

    private bool playerInside;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerUpgrades>() == null) return;

        playerInside = true;

        if (promptText != null)
        {
            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerUpgrades>() == null) return;

        playerInside = false;

        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(enterKey))
        {
            playerInside = false;
            GameSession.LoadDungeon();
        }
    }
}
