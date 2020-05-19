using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script handles velocity for the sliced up player model on gameover
 */
public class PlayerPiecesScript : MonoBehaviour
{
    private Transform[] playerPieces;                   // All the individual pieces of the model
    private float[] pieceSpeed;                         // Array of speed variables

    void Start()
    {
        playerPieces = new Transform[transform.childCount];
        pieceSpeed = new float[transform.childCount];
        for (int i = 0; i < playerPieces.Length; i++)
        {
            playerPieces[i] = transform.GetChild(i);
            pieceSpeed[i] = Random.Range(1, 100);
        }
    }

    void Update()
    {       
        // Loop makes all pieces rotate and float upwards
        for (int i = 0; i < playerPieces.Length; i++)
        {
            playerPieces[i].transform.position += Vector3.up * ((Time.deltaTime * pieceSpeed[i])/20);
            playerPieces[i].transform.Rotate(new Vector3(1 * 100f * Time.deltaTime, 0f, 1 * 20f * Time.deltaTime));
        }
    }
}
