using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody rb;

    public float arrowSpeed;
    public float slowSpeed;
    public float addedFlowForce;

    public bool hitObject;

    public GameObject ps;

    // Start is called before the first frame update
    void Start()
    {
        hitObject = false;
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * arrowSpeed, ForceMode.Impulse);
        rb.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hitObject)
        {
            
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        hitObject = true;
        //GameObject explosion = Instantiate(ps, transform.position, Quaternion.identity);
        //Destroy(explosion, 5f);
    }
}
