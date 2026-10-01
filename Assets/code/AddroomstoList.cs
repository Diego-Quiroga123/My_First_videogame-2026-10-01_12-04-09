using UnityEngine;

public class AddroomstoList : MonoBehaviour
{

    private Roomtemplates templates;
    
    void Start()
    {
        templates = GameObject.FindGameObjectWithTag("Rooms").GetComponent<Roomtemplates>();
        templates.rooms.Add(this.gameObject);
    }

    
}
