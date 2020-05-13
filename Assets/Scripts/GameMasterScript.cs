using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameMasterScript : MonoBehaviour
{
    public static GameMasterScript gameMasterScript;

    public static bool gameRunning;
    public static bool gameIsPaused;
    private bool gameIsOver;

    public GameObject pausePanel;
    public GameObject gameOverPanel;

    public GameObject player;
    public EnemyAI enemyAI;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI topScoreText;
    public TextMeshProUGUI anyButtonStartText;

    public readonly string scoreKey = "Score";
    public readonly string stageKey = "Stage";

    public GameObject[] minimalismObjects;

    private void Start()
    {
        gameMasterScript = this;
        enemyAI = EnemyAI.enemyAI;
        gameIsPaused = false;
        gameIsOver = false;
        anyButtonStartText.text = "Click any button to start";
        // Get volume and graphics stored in PlayerPrefs and set them
        if (PlayerPrefs.HasKey(MenuScript.graphicsKey))
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(MenuScript.graphicsKey));
        
        if (PlayerPrefs.HasKey(MenuScript.minimalismKey))
            minimalism();

        
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (!gameIsOver)
            {
                gameRunning = true;
                StartCoroutine(enemyAI.PhaseMachine());
                anyButtonStartText.text = "";
            }
        }
   

        if (gameRunning)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!gameIsPaused)
                    Pause();
                else
                    Resume();
            }
        }
    }

    private void minimalism()
    {

        if (PlayerPrefs.GetInt(MenuScript.minimalismKey) == 1)
            foreach (var item in minimalismObjects)
                item.SetActive(false);
        else
            foreach (var item in minimalismObjects)
                item.SetActive(true);
    }

    public void PlayAgain()
    {
        FadeTransition.fade.fadeTo(SceneManager.GetActiveScene().buildIndex);
    }


    public void GameOver()
    {
        gameRunning = false;
        gameIsOver = true;
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
        if (gameRunning)
        {
            pausePanel.SetActive(true);
            gameIsPaused = true;
            Time.timeScale = 0f;
        }
    }

    public void Resume()
    {
        if (gameRunning)
        {
            pausePanel.SetActive(false);
            gameIsPaused = false;
            Time.timeScale = 1f;
        }
    }

    public GameObject getPlayer()
    {
        return player;
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        FadeTransition.fade.fadeTo(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
