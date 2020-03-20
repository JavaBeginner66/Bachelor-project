using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributes : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;

    public Image healthDisplay;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("BulletHellObject"))
            takeDamage(StatsScript.BullethellDamage);
    }

    private void takeDamage(float damage)
    {
        currentHealth -= damage;
        healthDisplay.fillAmount = currentHealth / maxHealth;
    }
}
