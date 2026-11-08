using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public int openSide;

    //1 Need BottomDoor
    //2 Need TopDoor
    //3 Need LeftDoor
    //4 Need RightDoor

    private Roomtemplates templates;
    private bool spawned = false;

    void Start()
    {
        GameObject roomsObject = GameObject.FindGameObjectWithTag("Rooms");

        if (roomsObject == null || !roomsObject.TryGetComponent(out templates))
        {
            Debug.LogError("RoomSpawner: no hay un objeto con la etiqueta 'Rooms' que tenga el script Roomtemplates.", this);
            enabled = false;
            return;
        }

        Invoke(nameof(Spawn), 0.1f);
    }

    void Spawn()
    {
        if (spawned) return;

        GameObject[] options = null;

        switch (openSide)
        {
            case 1: options = templates.BottomDoor; break; // Need BottomDoor
            case 2: options = templates.TopDoor; break;    // Need TopDoor
            case 3: options = templates.LeftDoor; break;   // Need LeftDoor
            case 4: options = templates.RightDoor; break;  // Need RightDoor
        }

        if (options != null && options.Length > 0)
        {
            GameObject prefab = options[Random.Range(0, options.Length)];
            Instantiate(prefab, transform.position, prefab.transform.rotation);
        }

        spawned = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Antes: other.GetComponent<RoomSpawner>().spawned daba NullReferenceException
        // cada vez que entraba al trigger algo que no fuera otro RoomSpawner (jugador, paredes...)
        if (!other.TryGetComponent(out RoomSpawner otherSpawner)) return;

        if (!otherSpawner.spawned && !spawned)
        {
            if (templates.ClosedRoom != null)
            {
                Instantiate(templates.ClosedRoom, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }

        spawned = true;
    }
}
