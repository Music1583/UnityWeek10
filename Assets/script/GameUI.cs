using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("Hearts (Lives)")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite heartFullSprite;
    [SerializeField] private Sprite heartEmptySprite;

    [Header("Keys UI")]
    [SerializeField] private GameObject keysRoot;
    [SerializeField] private TMP_Text blueKeyText;
    [SerializeField] private TMP_Text greenKeyText;
    [SerializeField] private TMP_Text redKeyText;
    [SerializeField] private TMP_Text yellowKeyText;

    [Header("End Screen (Game Over / Level Complete)")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TMP_Text endScreenTitleText;
    [SerializeField] private Button endScreenActionButton;
    [SerializeField] private TMP_Text endScreenActionButtonText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Feedback Banner")]
    [SerializeField] private TMP_Text feedbackText;
    private Coroutine feedbackCoroutine;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (endScreenActionButton != null)
        {
            endScreenActionButton.onClick.AddListener(ReloadScene);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // Only allow pause if game over / level complete screen is not active
                if (endScreenPanel == null || !endScreenPanel.activeSelf)
                {
                    TogglePause();
                }
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                ReloadScene();
            }
        }
    }

    public void TogglePause()
    {
        SetPaused(!isPaused);
    }

    public void SetPaused(bool pause)
    {
        isPaused = pause;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            if (resumeButton != null)
            {
                SelectButton(resumeButton.gameObject);
            }
        }
        else
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public void OnResumeClicked()
    {
        SetPaused(false);
    }

    public void OnRestartClicked()
    {
        ReloadScene();
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void UpdateLives(int currentLives, int maxLives = 3)
    {
        if (heartImages == null) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            heartImages[i].sprite = (i < currentLives) ? heartFullSprite : heartEmptySprite;
        }
    }

    public void UpdateKeys(Dictionary<KeyColor, int> keys)
    {
        if (blueKeyText != null)
            blueKeyText.text = (keys.ContainsKey(KeyColor.Blue) ? keys[KeyColor.Blue] : 0).ToString();
        if (greenKeyText != null)
            greenKeyText.text = (keys.ContainsKey(KeyColor.Green) ? keys[KeyColor.Green] : 0).ToString();
        if (redKeyText != null)
            redKeyText.text = (keys.ContainsKey(KeyColor.Red) ? keys[KeyColor.Red] : 0).ToString();
        if (yellowKeyText != null)
            yellowKeyText.text = (keys.ContainsKey(KeyColor.Yellow) ? keys[KeyColor.Yellow] : 0).ToString();
    }

    public void ShowEndScreen(string title, string buttonLabel)
    {
        if (isPaused)
        {
            SetPaused(false);
        }

        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
        }

        if (endScreenTitleText != null)
        {
            endScreenTitleText.text = title;
        }

        if (endScreenActionButtonText != null)
        {
            endScreenActionButtonText.text = buttonLabel;
        }

        if (endScreenActionButton != null)
        {
            SelectButton(endScreenActionButton.gameObject);
        }
    }

    private void SelectButton(GameObject target)
    {
        if (EventSystem.current != null && target != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target);
        }
        StartCoroutine(SelectButtonRoutine(target));
    }

    private IEnumerator SelectButtonRoutine(GameObject target)
    {
        yield return null;
        if (EventSystem.current != null && target != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target);
        }
    }

    public void ReloadScene()
    {
        // Restore normal time scale before reloading
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void ShowFeedbackMessage(string message)
    {
        if (feedbackText == null) return;

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(FeedbackRoutine(message));
    }

    private IEnumerator FeedbackRoutine(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(2.5f);
        feedbackText.gameObject.SetActive(false);
    }
}
