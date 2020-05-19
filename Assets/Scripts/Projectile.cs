using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script is put on projectile prefab and carries a attack power value and speed
 * Projectile will act as a individual object carrying values from player to enemy
 */
public class Projectile : MonoBehaviour
{
    [Header("Inspector Objects")]
    public Rigidbody rb;                    // Rigidbody to add force to
    public GameObject onHitParticleSystem;  // Particlesystem that triggers on hit

    [Header("Modifiable variables")]
    public float projectileSpeed;           
    public float projectileDamage;           


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * projectileSpeed, ForceMode.Impulse);
        rb.useGravity = false;
    }

    public void setProjectileDamage(float dmg)
    {
        this.projectileDamage = dmg;
    }

    public float getProjectileDamage()
    {
        return this.projectileDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Boss1"))
        {
            Destroy(Instantiate(onHitParticleSystem, transform.position, Quaternion.identity), 3f);
            Destroy(gameObject);
        }
    }
}
