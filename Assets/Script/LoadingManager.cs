using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class LoadingManager : MonoBehaviour
{
    [Header("Cấu hình")]
    public string homeSceneName = "HomeScene"; 
    public float minimumLoadTime = 2f; 

    [Header("UI Loading (Sliced Image)")]
    public Image progressFill; 
    public float maxWidth = 500f; 
    public TextMeshProUGUI progressText;

    private async void Start()
    {
        await LoadAssetsAndSceneAsync();
    }

    private async Task LoadAssetsAndSceneAsync()
    {
        float elapsedTime = 0f;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(homeSceneName);
        
        if (asyncLoad == null)
        {
            Debug.LogError($"Không tìm thấy Scene '{homeSceneName}'!");
            return;
        }

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            elapsedTime += Time.deltaTime;

            float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            float fakeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            float displayProgress = Mathf.Min(realProgress, fakeProgress);

            if (progressFill != null)
            {
                progressFill.rectTransform.sizeDelta = new Vector2(maxWidth * displayProgress, progressFill.rectTransform.sizeDelta.y);
            }
            
            if (progressText != null) progressText.text = $"Loading... {Mathf.RoundToInt(displayProgress * 100)}%";

            if (asyncLoad.progress >= 0.9f && elapsedTime >= minimumLoadTime)
            {
                if (progressFill != null) 
                    progressFill.rectTransform.sizeDelta = new Vector2(maxWidth, progressFill.rectTransform.sizeDelta.y);
                
                if (progressText != null) progressText.text = "Done!";
                await Task.Delay(200);
                asyncLoad.allowSceneActivation = true;
            }

            await Task.Yield();
        }
    }
}
    