using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHellObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
            gameObject.SetActive(false);
    }
}
