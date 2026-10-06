using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverSummaryText;

    public Transform Player => playerHealth != null ? playerHealth.transform : null;
    public bool IsGameOver { get; private set; }
    private int score;
    private int highScore;

    private void Start()
    {
        Time.timeScale = 1f;
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        playerHealth.Changed += UpdateHealthBar;
        playerHealth.Died += OnPlayerDied;
        UpdateHealthBar(playerHealth);
        UpdateScoreText();
        gameOverPanel.SetActive(false);
    }

    public void AddScore(int amount)
    {
        if (IsGameOver)
        {
            return;
        }

        score += amount;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        UpdateScoreText();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
        highScoreText.text = $"High Score: {highScore}";
    }

    private void UpdateHealthBar(Health health)
    {
        healthFill.fillAmount = (float)health.Current / health.Max;
    }

    private void OnPlayerDied(Health health)
    {
        IsGameOver = true;
        gameOverSummaryText.text = $"GAME OVER\n\nFinal Score: {score}\nHigh Score: {highScore}";
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}