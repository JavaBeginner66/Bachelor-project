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
    public TextMeshProUGUI topScoreText;

    public readonly string scoreKey = "Score";
    public readonly string stageKey = "Stage";

    private void Start()
    {
        gameMasterScript = this;
        enemyAI = EnemyAI.enemyAI;
        gameIsPaused = false;
        //PlayerPrefs.DeleteAll();

        // Get volume and graphics stored in PlayerPrefs and set them
        if(PlayerPrefs.HasKey(MenuScript.graphicsKey))
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(MenuScript.graphicsKey));
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

        // Saving and getting highscore from playerprefs
        int stage = enemyAI.getPhase();
        int score = (int)enemyAI.getPlayerScore();
        string current = "Stage: " + stage + ",  " + score.ToString("N0") + " Points";
        string highest = "Highest score" + "\n" + "Stage: " + PlayerPrefs.GetInt(stageKey) + ",  Score: " + PlayerPrefs.GetInt(scoreKey);
        if (!PlayerPrefs.HasKey(scoreKey))
        {
            scoreText.text = "New topscore! " + "\n" +  current;
            PlayerPrefs.SetInt(stageKey, stage);
            PlayerPrefs.SetInt(scoreKey, score);
        }
        else
        {
            if(stage >= PlayerPrefs.GetInt(stageKey) && score > PlayerPrefs.GetInt(scoreKey))
            {
                scoreText.text = "New topscore! " + "\n" + current;
            }
            else
            {
                scoreText.text = current;
                topScoreText.text = highest;
            }
            
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
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
