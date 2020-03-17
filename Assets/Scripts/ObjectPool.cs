using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool objectPool;

    public readonly static string FrozenOrb = "FrozenOrb";
    public readonly static string BulletHellObject = "BulletHellObject";

    public GameObject bulletHellParent;
    public List<GameObject> pool;
    public GameObject frozenOrb;
    public GameObject bulletHellObject;

    public float frozenOrbAmount;
    public float bulletHellObjectAmount;

    // Start is called before the first frame update
    void Start()
    {
        objectPool = this;
        // Creating orbs with 20 bullets
        for(int i = 0; i<frozenOrbAmount; i++)
        {
            
            GameObject obj = Instantiate(frozenOrb);
            obj.transform.parent = bulletHellParent.transform;
            obj.SetActive(false);
            pool.Add(obj);
            for (int j = 0; j < bulletHellObjectAmount; j++)
            {
                GameObject obj2 = Instantiate(bulletHellObject);
                obj2.transform.parent = bulletHellParent.transform;
                obj2.SetActive(false);
                pool.Add(obj2);
            }
        }
    }

    public GameObject getStoredObject(string type)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if(pool[i].tag.Equals(type))
                if (!pool[i].activeInHierarchy)
                    return pool[i];
        }
        return null;
    }
}
