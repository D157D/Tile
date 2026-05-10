using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Game Manager")]
    public GameController gameManager;

    [Header("UI Panels")]
    public GameObject gamePanel;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("UI Buttons")]
    public Button nextLevelButton;
    public Button retryButton;
    public Button backToMenuButton;

    [Header("UI Texts")]
    public TextMeshProUGUI nextLevelButtonText;

    private int currentLevel = 1;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameController>();
        }

        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(NextLevel);
        if (retryButton != null) retryButton.onClick.AddListener(RetryLevel);
        if (backToMenuButton != null) backToMenuButton.onClick.AddListener(BackToMenu);

        if (gameManager != null)
        {
            gameManager.OnGameWon += ShowWinPanel;
            gameManager.OnGameLost += ShowLosePanel;
            gameManager.StartGame(currentLevel);
        }

        if (gamePanel != null) gamePanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    private void BackToMenu()
    {
        int prevSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (prevSceneIndex >= 0)
        {
            SceneManager.LoadScene(prevSceneIndex);
        }
    }

    private void NextLevel()
    {
        if (winPanel != null) winPanel.SetActive(false);
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RetryLevel()
    {
        if (losePanel != null) losePanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ShowWinPanel()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (nextLevelButtonText != null) nextLevelButtonText.text = "Level " + (currentLevel + 1);
    }

    private void ShowLosePanel()
    {
        if (losePanel != null) losePanel.SetActive(true);
    }
}
