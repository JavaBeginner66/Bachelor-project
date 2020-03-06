using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotate : MonoBehaviour
{
    public float rotateSpeed;

    void Update()
    {
        transform.Rotate(new Vector3(0f, 0f, rotateSpeed * Time.deltaTime));
    }
}
