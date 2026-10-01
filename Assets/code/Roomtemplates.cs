using UnityEngine;
using System.Collections.Generic;
public class Roomtemplates : MonoBehaviour
{
//salas
    public GameObject[] BottomDoor;
    public GameObject[] TopDoor;
    public GameObject[] LeftDoor;
    public GameObject[] RightDoor;

    public GameObject ClosedRoom;

//lista
    public List<GameObject> rooms;

//enemigos
    public GameObject Boss;
    public GameObject enemy;


    private void Start()
    {
        Invoke("spawnEnemies", 3f);
    }

    void spawnEnemies()
    {
        Instantiate(Boss, rooms[rooms.Count -1].transform.position, Quaternion.identity);

        for (int i = 0; i < rooms.Count -1; i++)
        {
            Instantiate(enemy, rooms[i].transform.position, Quaternion.identity);
        }
    }
}
