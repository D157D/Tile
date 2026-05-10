using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button playButton;

    [Header("UI Texts")]
    public TextMeshProUGUI playButtonText;

    void Start()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (playButtonText != null) playButtonText.text = "Level " + currentLevel;

        if (playButton != null) playButton.onClick.AddListener(PlayGame);
    }

    public void PlayGame()
    {
        // Load scene tiếp theo (Game Scene)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
