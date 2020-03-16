using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletOrb : MonoBehaviour
{
    public float scaleSpeed;
    // Update is called once per frame
    void Update()
    {
        if(transform.localScale.x < 30f)
            transform.localScale += new Vector3(scaleSpeed * Time.deltaTime, 0f, scaleSpeed * Time.deltaTime);
    }
}
