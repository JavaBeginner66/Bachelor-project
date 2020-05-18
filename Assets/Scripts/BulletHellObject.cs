using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Script is used on individual orb objects to detect if it hits player.
 */
public class BulletHellObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
            gameObject.SetActive(false);
    }
}
