﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    public readonly static string FrozenOrb = "FrozenOrb";
    public readonly static string FrozenOrbStatic = "FrozenOrbStatic";

    public List<GameObject> pool;

    public GameObject frozenOrb;
    public GameObject frozenOrbStatic;

    public float objectAmount;

    // Start is called before the first frame update
    void Start()
    {
        // Creating orbs with 20 bullets
        
        for (int i = 0; i<objectAmount; i++)
        {            
            GameObject fOrb = Instantiate(frozenOrb);
            GameObject fOrb2 = Instantiate(frozenOrbStatic);
            fOrb.SetActive(false);
            fOrb2.SetActive(false);
            pool.Add(fOrb);
            pool.Add(fOrb2);
            Debug.Log("pool");
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
