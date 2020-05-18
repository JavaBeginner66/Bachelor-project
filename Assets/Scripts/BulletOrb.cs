using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Script runs once everytime ObjectPool script instantiates a prefab.
 * Script is put on a invisible rotating/scaling gameobject(this) that holds individual orbs.
 */
public class BulletOrb : MonoBehaviour
{

    public GameObject bulletHellObject;             // Prefab of individual orb with a collider
    public List<GameObject> bulletHellObjectList;   // List of orbs

    public float scaleSpeed;                        // The speed of which the parent object(this) scales up
    public Transform[] orbPoints;                   // The location points on parent object(this) where orbs will instantiate

    public Transform movementObject;                // The grandparent object that takes care of movement
    public float movementObjectSpeed = 5f;          // Movement speed of parent object (this)
    public float maxTimer;                          // Countdown roof   
    public float endTimer;                          // Timer untill gameobject will disable itself

    /*
     * Awake is used to set up orb spawn positions, instantiate individual 
     * orbs, deactivating them and adding them to the list
     */
    private void Awake()
    {
        orbPoints = new Transform[transform.childCount];
        for (int i = 0; i < orbPoints.Length; i++)
        {
            orbPoints[i] = transform.GetChild(i);
            GameObject obj = Instantiate(bulletHellObject);
            obj.SetActive(false); 
            bulletHellObjectList.Add(obj);
        }   
    }


    /*
     * Method activates and positions individual orbs in place when gameobject is set to active.
     */
    private void OnEnable()
    {
        if (GameMasterScript.gameRunning)
        {
            endTimer = maxTimer;
            transform.localScale = new Vector3();
            for (int i = 0; i < orbPoints.Length; i++)
            {
                bulletHellObjectList[i].transform.position = orbPoints[i].transform.position;
                bulletHellObjectList[i].SetActive(true);
            }
        }
    }

    /*
     * Update method is used to controll gameobject
     */
    void Update()
    {
        // scale up the invisible gameobject(this)
        transform.localScale += new Vector3(scaleSpeed * Time.deltaTime, 0f, scaleSpeed * Time.deltaTime);
        // Move the invisible grandparent object forward
        movementObject.position += movementObject.forward * (movementObjectSpeed * Time.deltaTime);

        // Make sure the positions of the individual keeps in place
        for (int i = 0; i < orbPoints.Length; i++)
        {
            bulletHellObjectList[i].transform.position = orbPoints[i].transform.position;
        }

        // If timer reaches 0, disable everything
        endTimer -= Time.deltaTime;
        if (endTimer <= 0)
        {
            for (int i = 0; i < bulletHellObjectList.Count; i++)
            {
                bulletHellObjectList[i].SetActive(false);
            }
            transform.parent.gameObject.SetActive(false);
        }
    }
}
