using TMPro;
using UnityEngine;

/// <summary>
/// Muestra en pantalla los materiales del inventario y el nivel del equipo.
/// Ponlo en cualquier objeto del Canvas y arrastra uno o los dos textos.
/// </summary>
public class MaterialCounterUI : MonoBehaviour
{
    [Header("Jugador (si lo dejas vacío, lo busca por el tag 'Player')")]
    [SerializeField] private PlayerUpgrades player;

    [Header("Textos (los dos son opcionales)")]
    [SerializeField] private TMP_Text materialsText;
    [SerializeField] private TMP_Text levelText;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.GetComponent<PlayerUpgrades>();
        }

        if (player == null)
        {
            Debug.LogError("MaterialCounterUI: no encuentro al jugador con el script PlayerUpgrades.", this);
            enabled = false;
            return;
        }

        player.onItemsChanged.AddListener(OnChanged);
        player.onEquipmentLevelUp.AddListener(OnChanged);

        Refresh();
    }

    private void OnDestroy()
    {
        if (player == null) return;

        player.onItemsChanged.RemoveListener(OnChanged);
        player.onEquipmentLevelUp.RemoveListener(OnChanged);
    }

    private void OnChanged(int _)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (materialsText != null) materialsText.text = "Materiales: " + player.Materials;
        if (levelText != null) levelText.text = "Nivel: " + player.EquipmentLevel + " / " + player.MaxEquipmentLevel;
    }
}