using System;
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
    private bool isChoosingSkill;
    private GameObject skillSelectionPanel;
    private Canvas hudCanvas;

    private void Awake()
    {
        TMP_FontAsset fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (fallbackFont == null)
        {
            throw new System.InvalidOperationException("TMP Essentials font asset is missing from Resources.");
        }

        hudCanvas = scoreText.GetComponentInParent<Canvas>();
        foreach (TMP_Text text in hudCanvas.GetComponentsInChildren<TMP_Text>(true))
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

        foreach (Image image in hudCanvas.GetComponentsInChildren<Image>(true))
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
        if (!isChoosingSkill && gameStarted && !IsGameOver && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
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

    public void ShowSkillChoices(PlayerController.Upgrade[] choices, Action<PlayerController.Upgrade> onChosen)
    {
        if (choices == null || choices.Length != 3)
        {
            throw new ArgumentException("Exactly three skill choices are required.", nameof(choices));
        }

        if (skillSelectionPanel == null)
        {
            skillSelectionPanel = CreateSkillSelectionPanel();
        }

        skillSelectionPanel.SetActive(true);
        isChoosingSkill = true;
        Time.timeScale = 0f;

        RectTransform panel = skillSelectionPanel.GetComponent<RectTransform>();
        for (int i = 0; i < 3; i++)
        {
            int choiceIndex = i;
            Button button = panel.GetChild(i + 2).GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text =
                $"{PlayerController.GetUpgradeTitle(choices[i])}\n\n{PlayerController.GetUpgradeDescription(choices[i])}";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                skillSelectionPanel.SetActive(false);
                isChoosingSkill = false;
                onChosen(choices[choiceIndex]);
                Time.timeScale = 1f;
            });
        }
    }

    private GameObject CreateSkillSelectionPanel()
    {
        GameObject panelObject = new GameObject("Skill Selection", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(hudCanvas.transform, false);
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(1100f, 480f);
        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.sprite = fallbackImageSprite;
        panelImage.color = new Color(.025f, .08f, .09f, .96f);

        CreateSkillText(panel, "Choose a skill", 42f, new Vector2(0f, 165f), new Vector2(950f, 70f), FontStyles.Bold);
        CreateSkillText(panel, "Wave cleared - full health restored", 24f, new Vector2(0f, 112f), new Vector2(950f, 45f), FontStyles.Normal);

        for (int i = 0; i < 3; i++)
        {
            GameObject buttonObject = new GameObject($"Skill Choice {i + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(panel, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2((i - 1) * 355f, -35f);
            buttonRect.sizeDelta = new Vector2(320f, 250f);
            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.sprite = fallbackImageSprite;
            buttonImage.color = new Color(.08f, .38f, .4f, 1f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(.12f, .55f, .58f, 1f);
            colors.pressedColor = new Color(.05f, .27f, .29f, 1f);
            button.colors = colors;
            CreateSkillText(buttonRect, "", 25f, Vector2.zero, new Vector2(280f, 210f), FontStyles.Bold);
        }

        return panelObject;
    }

    private TMP_Text CreateSkillText(Transform parent, string value, float size, Vector2 position, Vector2 dimensions, FontStyles style)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.font = scoreText.font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
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