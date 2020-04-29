using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMasterScript : MonoBehaviour
{
    public static bool gameRunning;

    public GameObject player;
    public EnemyAI enemyAI;

    private void Start()
    {
        enemyAI = EnemyAI.enemyAI;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            gameRunning = true;

        if (Input.GetKeyDown(KeyCode.V))
            enemyAI.nextPhase();

        if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine (enemyAI.PhaseMachine());
    }

    public GameObject getPlayer()
    {
        return player;
    }
}
