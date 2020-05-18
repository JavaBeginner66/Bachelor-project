using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script is used as a generic way of rotating objects its put on
 */
public class ObjectRotate : MonoBehaviour
{
    public Vector3 values;
    public float rotateSpeed;

    void Update()
    {
        transform.Rotate(new Vector3(values.x * rotateSpeed * Time.deltaTime, values.y * rotateSpeed * Time.deltaTime, values.z * rotateSpeed * Time.deltaTime));
    }
}
