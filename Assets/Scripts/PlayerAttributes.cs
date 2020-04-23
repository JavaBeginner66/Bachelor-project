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
            takeDamage();

        if (other.tag.Equals("TargetCircle"))
            takeDamage();

        if (other.tag.Equals("QuarterCircle"))
            takeDamage();

        if (other.tag.Equals("RotatingWalls"))
            takeDamage();
    }

    private void takeDamage()
    {
        Debug.Log("Damage taken");
        playerMovement.playerShieldDamage();
    }
}
