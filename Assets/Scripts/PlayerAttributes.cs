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

        if (other.tag.Equals("TargetCircle"))
            takeDamage(10f);

        if (other.tag.Equals("QuarterCircle"))
            takeDamage(10f);
    }

    private void takeDamage(float damage)
    {
        Debug.Log("Damage taken");
        playerMovement.playerShieldDamage();
    }
}
