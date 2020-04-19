using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPositions : MonoBehaviour
{

    private Transform[] positions;
    public Transform player;
    public Vector3 offset;

    void Start()
    {
        positions = new Transform[transform.childCount];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.GetChild(i);
            positions[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        transform.position = player.position + offset;

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i].LookAt(transform);
        }
    }
}
