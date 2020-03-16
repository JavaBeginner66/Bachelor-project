using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletOrb : MonoBehaviour
{
    public float scaleSpeed;
    public Transform[] orbPoints;
    public GameObject[] orbs;

    public GameObject orbPrefab;

    private void Start()
    {
        orbPoints = new Transform[transform.childCount];
        orbs = new GameObject[orbPoints.Length];
        for (int i = 0; i < transform.childCount; i++)
        {
            orbPoints[i] = transform.GetChild(i);
            orbs[i] =  Instantiate(orbPrefab);
        }

    }
    void Update()
    {
        if(transform.localScale.x < 30f)
            transform.localScale += new Vector3(scaleSpeed * Time.deltaTime, 0f, scaleSpeed * Time.deltaTime);

        for (int i = 0; i < orbPoints.Length; i++)
        {
            orbs[i].transform.position = orbPoints[i].transform.position;
        }
    }
}
