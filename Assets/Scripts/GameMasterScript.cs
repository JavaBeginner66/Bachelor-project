using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMasterScript : MonoBehaviour
{
    public static bool gameRunning;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            gameRunning = true;

        if (Input.GetKeyDown(KeyCode.V))
            gameRunning = false;
    }
}
