using UnityEngine;

/// <summary>
/// Ponle este script al prefab del objeto que sueltan los enemigos.
/// El prefab necesita un Collider con "Is Trigger" activado.
/// Lo recoge quien tenga el script PlayerUpgrades (ya no hace falta el tag "Player").
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public int value = 1; // Cuántos "objetos" cuenta al recogerlo

    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return; // Destroy se aplica al final del frame: evita contar dos veces

        PlayerUpgrades upgrades = other.GetComponentInParent<PlayerUpgrades>();
        if (upgrades == null) return;

        collected = true;
        upgrades.AddItems(value);
        Destroy(gameObject);
    }
}
