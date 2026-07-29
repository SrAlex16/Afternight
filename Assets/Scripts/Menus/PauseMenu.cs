using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused { get; private set; }

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Sound params")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip menuClip;

    private void Update()
    {
        if (!Input.GetKeyDown(pauseKey)) return;

        PlaySafe();
        if (GameIsPaused) Resume();
        else Pause();
    }

    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void CloseApp() => Application.Quit();

    private void PlaySafe()
    {
        if (audioSource != null && menuClip != null) audioSource.PlayOneShot(menuClip);
    }
}
