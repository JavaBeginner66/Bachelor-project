using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPiecesScript : MonoBehaviour
{
    private Transform[] playerPieces;
    private float[] pieceSpeed;
    // Start is called before the first frame update
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
        for (int i = 0; i < playerPieces.Length; i++)
        {
            playerPieces[i].transform.position += Vector3.up * ((Time.deltaTime * pieceSpeed[i])/20);
            playerPieces[i].transform.Rotate(new Vector3(1 * 100f * Time.deltaTime, 0f, 1 * 20f * Time.deltaTime));
        }
    }
}
