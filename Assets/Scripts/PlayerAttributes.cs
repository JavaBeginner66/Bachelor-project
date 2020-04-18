using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributes : MonoBehaviour
{
    public PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(StatsScript.BulletHellDamageTag))
            takeDamage(StatsScript.BullethellDamage);
    }

    private void takeDamage(float damage)
    {
        playerMovement.playerShieldDamage();
    }
}
