using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameMasterScript : MonoBehaviour
{
    public static GameMasterScript gameMasterScript;

    public static bool gameRunning;
    public static bool gameIsPaused;

    public GameObject pausePanel;
    public GameObject gameOverPanel;

    public GameObject player;
    public EnemyAI enemyAI;

    public TextMeshProUGUI scoreText;

    private void Start()
    {
        gameMasterScript = this;
        enemyAI = EnemyAI.enemyAI;
        gameIsPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            gameRunning = true;

        if (Input.GetKeyDown(KeyCode.V))
            gameRunning = false;

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

    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }

    public void GameOver()
    {
        gameRunning = false;
        gameOverPanel.SetActive(true);
        scoreText.text = "Stage: " + enemyAI.getPhase() +"  " + enemyAI.getPlayerScore().ToString() + " Points";       
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
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
