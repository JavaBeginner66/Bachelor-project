using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Inspector Objects")]
    public Rigidbody rb;
    public GameObject onHitParticleSystem;

    [Header("Modifiable variables")]
    public float projectileSpeed;
    public float projectileDamage;
    
    [HideInInspector]
    public bool hitObject;



    // Start is called before the first frame update
    void Start()
    {
        
        hitObject = false;
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * projectileSpeed, ForceMode.Impulse);
        rb.useGravity = false;
    }

    /**
     * Projectile will act as a individual object carrying values from player to enemy
     * */
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
