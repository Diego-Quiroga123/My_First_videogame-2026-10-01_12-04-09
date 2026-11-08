using UnityEngine;

/// <summary>
/// Puerta que aparece al derrotar al jefe. Necesita un Collider con "Is Trigger" activado.
/// Cuando el jugador la toca, abre el menú de salida (ExitMenu): volver a la base o siguiente piso.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitDoor : MonoBehaviour
{
    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (other.GetComponentInParent<PlayerUpgrades>() == null) return; // solo el jugador

        if (ExitMenu.Instance == null)
        {
            Debug.LogError("ExitDoor: no hay un ExitMenu en la escena. Crea el menú de salida en el Canvas.", this);
            return;
        }

        used = true;
        ExitMenu.Instance.Show();
    }
}
