using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPointMenu : MonoBehaviour
{
    [SerializeField] private GameObject checkPointMenuUI;
    [SerializeField] private string sceneName;
    [SerializeField] private string targetTag = "Player";

    [Header("Sound params")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip menuClip;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;

        if (audioSource != null && menuClip != null) audioSource.PlayOneShot(menuClip);
        Pause();
    }

    public void Pause()
    {
        if (checkPointMenuUI != null) checkPointMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void NextLevel()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            GameLogger.Error(GameLogger.Category.UI, $"{name}: sceneName vacío, no se puede cargar el siguiente nivel.", this);
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void CloseApp() => Application.Quit();
}
