using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/*
 * Script handles status of game and different UI functions
 */
public class GameMasterScript : MonoBehaviour
{
    public static GameMasterScript gameMasterScript;    // Static reference for easy access

    public static bool gameRunning;                     // Status of game
    public static bool gameIsPaused;                    // True if game is paused
    private bool gameIsOver;                            // True if player is dead

    public GameObject pausePanel;                       // Pause panel UI object
    public GameObject gameOverPanel;                    // Game over panel UI object

    public GameObject player;                           // Player object reference
    public EnemyAI enemyAI;                             // Boss object reference

    public TextMeshProUGUI scoreText;                   // Score text displayed on game over screen
    public TextMeshProUGUI topScoreText;                // Top score text displayed on game over screen
    public TextMeshProUGUI anyButtonStartText;          // "Click any button to start" text at the start of game

    public readonly string scoreKey = "Score";          // Key for saving score in PlayerPrefs
    public readonly string stageKey = "Stage";          // Key for saving stage in PlayerPrefs

    public GameObject[] minimalismObjects;              // Array of objects that will disappear/appear from scene 

    /*
     * Start sets up references and checks if PlayerPrefs has existing keys
     */
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

    /*
     * Update is used to detect player click to start the game, and "esc" to pause it
     */
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

    /*
     * Method removes/adds some of the objects from the scene
     */
    private void minimalism()
    {

        if (PlayerPrefs.GetInt(MenuScript.minimalismKey) == 1)
            foreach (var item in minimalismObjects)
                item.SetActive(false);
        else
            foreach (var item in minimalismObjects)
                item.SetActive(true);
    }

    /*
     * Method listens for click on "Play again" on game over screen, and reloads the scene through FadeTransition script
     */
    public void PlayAgain()
    {
        FadeTransition.fade.fadeTo(SceneManager.GetActiveScene().buildIndex);
    }

    /*
     * Method stops all game activity, and gets scores to show player in gameover screen
     */
    public void GameOver()
    {
        gameRunning = false;
        gameIsOver = true;
        gameOverPanel.SetActive(true);

        // Saving and getting highscore from playerprefs
        int stage = enemyAI.getPhase();
        int score = (int)enemyAI.getPlayerScore();
        // Building the strings
        string current = "Stage: " + stage + ",  " + score.ToString("N0") + " Points";
        string highest = "Highest score" + "\n" + "Stage: " + PlayerPrefs.GetInt(stageKey) + ",  Score: " + PlayerPrefs.GetInt(scoreKey);
        // If Playerprefs doesn't already have a key stores, the player plays for the first time and it will be a topscore
        if (!PlayerPrefs.HasKey(scoreKey))
        {
            scoreText.text = "New topscore! " + "\n" +  current;
            PlayerPrefs.SetInt(stageKey, stage);
            PlayerPrefs.SetInt(scoreKey, score);
        }
        else
        {
            // If a key exist, check current score compared to previous
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

    /*
     * Method pauses the game and freezes gametime
     */
    public void Pause()
    {
        if (gameRunning)
        {
            pausePanel.SetActive(true);
            gameIsPaused = true;
            Time.timeScale = 0f;
        }
    }

    /*
     * Method resumes game and unfreezes gametime
     */
    public void Resume()
    {
        if (gameRunning)
        {
            pausePanel.SetActive(false);
            gameIsPaused = false;
            Time.timeScale = 1f;
        }
    }

    /*
     * Method detects click on "Menu" on either pause screen or gameover screen, 
     * and changes scene to Menu trough FadeTransition script
     */
    public void Menu()
    {
        Time.timeScale = 1f;
        FadeTransition.fade.fadeTo(0);
    }

    /*
     * Method detects click on "Quit" and quits game
     */
    public void QuitGame()
    {
        Application.Quit();
    }
    
    /*
     * Get method
     */
    public GameObject getPlayer()
    {
        return player;
    }
}
