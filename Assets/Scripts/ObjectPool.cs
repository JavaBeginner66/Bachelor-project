using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
     Script instantiates objects(prefabs) and keeps track of them in a list.
     This is the top level of object pool, and only instatiates 2 variants that each has their 
     own script with a object pool. (See script BulletOrb)
*/
public class ObjectPool : MonoBehaviour
{

    public readonly static string FrozenOrb = "FrozenOrb";
    public readonly static string FrozenOrbStatic = "FrozenOrbStatic";

    public List<GameObject> pool;       // List that manages the objects

    public GameObject frozenOrb;        // Prefab variant. Script on prefab has it's own object pool which runs on instantiation.
    public GameObject frozenOrbStatic;  // Prefab variant. Script on prefab has it's own object pool which runs on instantiation.

    public float objectAmount;          // Amount of object to instantiate set in editor

    void Start()
    {     
        for (int i = 0; i<objectAmount; i++)
        {   
            // Instantiate prefab, deactivate and add to list.
            GameObject fOrb = Instantiate(frozenOrb);
            GameObject fOrb2 = Instantiate(frozenOrbStatic);
            fOrb.SetActive(false);
            fOrb2.SetActive(false);
            pool.Add(fOrb);
            pool.Add(fOrb2);
        }
    }

    /**
     * Method goes through pools list and checks whether the object is active in the scene.
     * If not, get it and return it. Activation and positioning is dealt with by enemyAI script.
     */
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
