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

    public GameObject[] minimalismObjects;

    private void Start()
    {
        gameMasterScript = this;
        enemyAI = EnemyAI.enemyAI;
        gameIsPaused = false;

        // Get volume and graphics stored in PlayerPrefs and set them
        if(PlayerPrefs.HasKey(MenuScript.graphicsKey))
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(MenuScript.graphicsKey));
        
        if (PlayerPrefs.HasKey(MenuScript.minimalismKey))
            minimalism();

        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            gameRunning = true;

        if (Input.GetKeyDown(KeyCode.V))
            gameRunning = false;

        if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine (enemyAI.PhaseMachine());

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
        {
            foreach (var item in minimalismObjects)
                item.SetActive(false);

            Debug.Log("on");
        }
        else
        {
            foreach (var item in minimalismObjects)
                item.SetActive(true);

            Debug.Log("off");
        }
    }

    public void PlayAgain()
    {
        FadeTransition.fade.fadeTo(SceneManager.GetActiveScene().buildIndex);
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
