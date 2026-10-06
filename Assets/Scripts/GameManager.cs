using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static Sprite fallbackImageSprite;

    [SerializeField] private Health playerHealth;
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverSummaryText;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private AudioSource effectsSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip zombieDeathClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip musicClip;

    public Transform Player => playerHealth != null ? playerHealth.transform : null;
    public bool IsGameOver { get; private set; }
    private static bool launchImmediately;
    private int score;
    private int highScore;
    private int previousPlayerHealth = -1;
    private bool gameStarted;
    private bool isPaused;

    private void Awake()
    {
        TMP_FontAsset fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (fallbackFont == null)
        {
            throw new System.InvalidOperationException("TMP Essentials font asset is missing from Resources.");
        }

        Canvas canvas = scoreText.GetComponentInParent<Canvas>();
        foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.font == null)
            {
                text.font = fallbackFont;
            }
        }

        if (fallbackImageSprite == null)
        {
            fallbackImageSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f));
        }

        foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
        {
            if (image.sprite == null)
            {
                image.sprite = fallbackImageSprite;
            }
        }
    }

    private void Start()
    {
        gameStarted = launchImmediately;
        launchImmediately = false;
        Time.timeScale = gameStarted ? 1f : 0f;
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        playerHealth.Changed += UpdateHealthBar;
        playerHealth.Died += OnPlayerDied;
        UpdateHealthBar(playerHealth);
        UpdateScoreText();
        gameOverPanel.SetActive(false);
        mainMenuPanel.SetActive(!gameStarted);
        pauseMenuPanel.SetActive(false);
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayShootSound() => effectsSource.PlayOneShot(shootClip);
    public void PlayHitSound() => effectsSource.PlayOneShot(hitClip);
    public void PlayZombieDeathSound() => effectsSource.PlayOneShot(zombieDeathClip);

    private void Update()
    {
        if (gameStarted && !IsGameOver && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        if (!gameStarted || IsGameOver || isPaused)
        {
            return;
        }

        isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
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
        launchImmediately = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
        highScoreText.text = $"High Score: {highScore}";
    }

    private void UpdateHealthBar(Health health)
    {
        healthFill.fillAmount = (float)health.Current / health.Max;
        if (previousPlayerHealth >= 0 && health.Current < previousPlayerHealth)
        {
            health.GetComponent<PlayerController>()?.ShakeCamera();
        }
        previousPlayerHealth = health.Current;
    }

    private void OnPlayerDied(Health health)
    {
        IsGameOver = true;
        pauseMenuPanel.SetActive(false);
        effectsSource.PlayOneShot(gameOverClip);
        gameOverSummaryText.text = $"GAME OVER\n\nFinal Score: {score}\nHigh Score: {highScore}";
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}