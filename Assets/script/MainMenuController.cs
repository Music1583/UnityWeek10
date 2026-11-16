using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (playButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Platformer_Start");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
