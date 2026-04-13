using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float gameTime;
    public bool gameActive;
    
    [Header("Kill Tracking")]
    public int totalKills;
    public int currentWaveKills;
    
    [Header("Points System")]
    public int totalPoints;
    public int currentWavePoints;
    [SerializeField] private int waveCompletionBonus = 500;
    [SerializeField] private float bonusMultiplierPerCycle = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void Start()
    {
        gameActive = true;
        totalKills = 0;
        currentWaveKills = 0;
        totalPoints = 0;
        currentWavePoints = 0;
        // Do NOT reset timeScale here — PlayerController.Start() opens
        // the level up panel with timeScale = 0f for initial weapon selection
    }
    
    void Update()
    { 
        if (gameActive)
        {
            gameTime += Time.deltaTime;
            if (UIController.Instance != null)
            {
                UIController.Instance.UpdateTimer(gameTime);
            }

            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)))
            {
                Pause();
            }
        }
    }

    public void AddKill()
    {
        totalKills++;
        currentWaveKills++;
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateKillCount();
        }
    }

    public void AddPoints(int points)
    {
        totalPoints += points;
        currentWavePoints += points;
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdatePointsDisplay();
        }
    }

    public void AwardWaveCompletionBonus(int waveCycle)
    {
        int bonus = Mathf.RoundToInt(waveCompletionBonus * Mathf.Pow(bonusMultiplierPerCycle, waveCycle));
        totalPoints += bonus;
        
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdatePointsDisplay();
            UIController.Instance.ShowWaveBonusNotification(bonus);
        }
    }

    public void ResetWaveKills()
    {
        currentWaveKills = 0;
        currentWavePoints = 0;
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateKillCount();
        }
    }

    public void GameOver() { 
        gameActive = false;
        StartCoroutine(ShowGameOverScreen());
    }
    
    IEnumerator ShowGameOverScreen() { 
        yield return new WaitForSeconds(1.5f);
        if (UIController.Instance != null)
        {
            UIController.Instance.gameOverPanel.SetActive(true);
        }
    }

    public void Restart() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }
    
    public void Pause()
    {
        if (UIController.Instance == null)
        {
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
            return;
        }

        if (UIController.Instance.pausePanel.activeSelf == false && UIController.Instance.gameOverPanel.activeSelf == false)
        {
            UIController.Instance.pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            UIController.Instance.pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
    
    public void Quit()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
