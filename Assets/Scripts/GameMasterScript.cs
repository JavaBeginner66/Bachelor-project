using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMasterScript : MonoBehaviour
{
    public static bool gameRunning;
    public static bool gameIsPaused;

    public GameObject pausePanel;

    public GameObject player;
    public EnemyAI enemyAI;

    private void Start()
    {
        enemyAI = EnemyAI.enemyAI;
        gameIsPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            gameRunning = true;

        if (Input.GetKeyDown(KeyCode.V))
            enemyAI.nextPhase();

        if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine (enemyAI.PhaseMachine());

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameIsPaused)
                Pause();
            else
                Resume();
        }      
    }

    public void Pause()
    {
        
        pausePanel.SetActive(true);
        gameIsPaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        gameIsPaused = false;
        Time.timeScale = 1f;
    }

    public GameObject getPlayer()
    {
        return player;
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
