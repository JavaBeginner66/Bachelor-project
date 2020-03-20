using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public float maxHealth;
    public float health;

    private void Start()
    {
        health = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("BulletHellObject"))
            takeDamage(StatsScript.BullethellDamage);
    }

    private void takeDamage(float damage)
    {
        health -= damage;
        Debug.Log("PlayerHealth: " + health);
    }
}
