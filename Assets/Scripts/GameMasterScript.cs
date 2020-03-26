using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMasterScript : MonoBehaviour
{
    public static bool gameRunning;

    public GameObject player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            gameRunning = true;

        if (Input.GetKeyDown(KeyCode.V))
            gameRunning = false;
    }

    public GameObject getPlayer()
    {
        return player;
    }
}
