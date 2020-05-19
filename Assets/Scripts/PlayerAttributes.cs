using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Script checks what player is hit by, and if it should be damaging
 */
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
            takeDamage();

        if (other.tag.Equals(StatsScript.TargetCircle))
            takeDamage();

        if (other.tag.Equals(StatsScript.QuarterCircle))
            takeDamage();

        if (other.tag.Equals(StatsScript.RotatingWalls))
            takeDamage();
    }

    private void takeDamage()
    {
        playerMovement.playerShieldDamage();
    }
}
