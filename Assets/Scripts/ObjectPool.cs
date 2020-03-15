using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool objectPool;

    public List<GameObject> pool;
    public GameObject poolObject;

    public float poolAmount;
    // Start is called before the first frame update
    void Start()
    {
        objectPool = this;
        for(int i = 0; i<poolAmount; i++)
        {
            GameObject obj = Instantiate(poolObject);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject getStoredObject()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
                return pool[i];
        }
        return null;
    }
}
