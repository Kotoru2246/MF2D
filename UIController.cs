using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    [SerializeField] private Slider playerHealthSlider;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject levelUpPanel;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Slider playerExperienceSlider;
    [SerializeField] private TMP_Text playerLevelText;
    
    [Header("Kill Counter UI")]
    [SerializeField] private TMP_Text totalKillsText;
    [SerializeField] private TMP_Text waveKillsText;
    [SerializeField] private TMP_Text waveNumberText;
    
    [Header("Points UI")]
    [SerializeField] private TMP_Text totalPointsText;
    [SerializeField] private TMP_Text waveBonusText; 
    [SerializeField] private float bonusDisplayDuration = 2f;

    public LevelUpButton[] levelUpButtons;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Ensure wave bonus text starts inactive
        if (waveBonusText != null)
        {
            waveBonusText.gameObject.SetActive(false);
        }
    }

    
    public void UpdatePlayerHealthSlider()
    {
        playerHealthSlider.maxValue = PlayerController.Instance.playerMaxHealth;
        playerHealthSlider.value = PlayerController.Instance.playerCurrentHealth;
    }
    
    public void UpdatePlayerExperienceSlider()
    {
        playerExperienceSlider.maxValue = PlayerController.Instance.playerLevels[PlayerController.Instance.currentLevel - 1];
        
        playerExperienceSlider.value = PlayerController.Instance.experience;
        playerLevelText.text = PlayerController.Instance.currentLevel.ToString();
    }

    public void UpdateTimer(float timer)
    {
        float min = Mathf.FloorToInt(timer / 60);
        float sec = Mathf.FloorToInt(timer % 60);
        timerText.text = min + ":" + sec.ToString("00");
    }

    public void UpdateKillCount()
    {
        if (totalKillsText != null)
        {
            totalKillsText.text = "Kills: " + GameManager.Instance.totalKills;
        }
        
        if (waveKillsText != null)
        {
            waveKillsText.text = "Wave Kills: " + GameManager.Instance.currentWaveKills;
        }
    }

    public void UpdatePointsDisplay()
    {
        if (totalPointsText != null)
        {
            totalPointsText.text = "Points: " + GameManager.Instance.totalPoints;
        }
    }

    public void ShowWaveBonusNotification(int bonus)
    {
        if (waveBonusText != null)
        {
            StartCoroutine(DisplayWaveBonus(bonus));
        }
    }

    private IEnumerator DisplayWaveBonus(int bonus)
    {
        // Set text and activate GameObject
        waveBonusText.text = "Wave Cleared! +" + bonus + " Points!";
        waveBonusText.gameObject.SetActive(true);
        
        // Use WaitForSecondsRealtime to ignore time scale (works even when paused)
        yield return new WaitForSecondsRealtime(bonusDisplayDuration);
        
        // Deactivate GameObject
        waveBonusText.gameObject.SetActive(false);
    }

    public void UpdateWaveNumber(int waveNumber)
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = "Wave: " + (waveNumber + 1);
        }
    }

    public void LevelUpPanelOpen() { 
        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LevelUpPanelClose() { 
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}