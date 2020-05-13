using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletOrb : MonoBehaviour
{

    public GameObject bulletHellObject;
    public List<GameObject> bulletHellObjectList;

    public float scaleSpeed;
    public Transform[] orbPoints;

    public Transform movementObject;
    public float movementObjectSpeed = 5f;
    public float maxTimer;
    public float endTimer;

    private void Awake()
    {
        orbPoints = new Transform[transform.childCount];
        for (int i = 0; i < orbPoints.Length; i++)
        {
            orbPoints[i] = transform.GetChild(i);
            GameObject obj = Instantiate(bulletHellObject);
            obj.SetActive(false); // Hvorfor er de aktiverte....
            bulletHellObjectList.Add(obj);
            Debug.Log("bullet");
        }   
    }



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

    void Update()
    {

        transform.localScale += new Vector3(scaleSpeed * Time.deltaTime, 0f, scaleSpeed * Time.deltaTime);


        movementObject.position += movementObject.forward * (movementObjectSpeed * Time.deltaTime);


        for (int i = 0; i < orbPoints.Length; i++)
        {
            bulletHellObjectList[i].transform.position = orbPoints[i].transform.position;
        }

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
