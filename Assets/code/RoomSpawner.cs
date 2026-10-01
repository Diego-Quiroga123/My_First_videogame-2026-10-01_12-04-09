using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public int openSide;

    //1 Need BottomDoor
    //2 Need TopDoor
    //3 Need LeftDoor
    //4 Need RightDoor

    private Roomtemplates templates;
    private int rand;
    private bool spawned = false;



    void Start()
    {
        templates = GameObject.FindGameObjectWithTag("Rooms").GetComponent<Roomtemplates>();
        Invoke("Spawn", 0.1f);
    }


    void Spawn()
    {

        if (spawned == false)
        {
            if(openSide == 1)
        {
             //Need BottomDoor
             rand = Random.Range(0, templates.BottomDoor.Length);
             Instantiate(templates.BottomDoor[rand], transform.position, templates.BottomDoor[rand].transform.rotation);
             

        }
        else if(openSide == 2)
        {
            //2 Need TopDoor
            rand = Random.Range(0, templates.TopDoor.Length);
            Instantiate(templates.TopDoor[rand], transform.position, templates.TopDoor[rand].transform.rotation);
        }
        else if(openSide == 3)
        {
            //3 Need LeftDoor
            rand = Random.Range(0, templates.LeftDoor.Length);
            Instantiate(templates.LeftDoor[rand], transform.position, templates.LeftDoor[rand].transform.rotation);
        }
        else if(openSide == 4)
        {
            //4 Need RightDoor
            rand = Random.Range(0, templates.RightDoor.Length);
            Instantiate(templates.RightDoor[rand], transform.position, templates.RightDoor[rand].transform.rotation);
        }   
        spawned = true;
        }
        
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<RoomSpawner>().spawned == false && spawned == false)
            {
                Instantiate(templates.ClosedRoom, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
            spawned = true;
        }
    }



