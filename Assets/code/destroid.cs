using UnityEngine;

public class destroid : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Antes destruía CUALQUIER cosa que entrara, incluido el jugador y los enemigos.
        if (other.CompareTag("Player") || other.GetComponentInParent<Health>() != null) return;

        Destroy(other.gameObject);
    }
}
