using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class LoadingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI loadingText;
    
    [Header("Settings")]
    [SerializeField] private float loadingTime = 2f;
    [SerializeField] private float dotAnimationSpeed = 0.5f;
    
    private string targetScene;
    private string baseText = "Loading";
    
    private void Start()
    {
        // Get target scene
        targetScene = PlayerPrefs.GetString("TargetScene", "GameScene");
        PlayerPrefs.DeleteKey("TargetScene");
        
        StartLoading();
    }
    
    private void StartLoading()
    {
        // Setup initial state
        if (loadingText != null)
        {
            loadingText.text = baseText;
            StartTextAnimation();
        }
        
        // Start loading process
        StartCoroutine(LoadingProcess());
    }
    
    private void StartTextAnimation()
    {
        // Animate loading text with dots
        StartCoroutine(AnimateLoadingDots());
    }
    
    private IEnumerator AnimateLoadingDots()
    {
        int dotCount = 0;
        
        while (true)
        {
            dotCount = (dotCount + 1) % 4; // 0, 1, 2, 3, then back to 0
            
            string dots = new string('.', dotCount);
            if (loadingText != null)
                loadingText.text = baseText + dots;
            
            yield return new WaitForSeconds(dotAnimationSpeed);
        }
    }
    
    private IEnumerator LoadingProcess()
    {
        float elapsed = 0f;
        
        // Wait for loading time
        while (elapsed < loadingTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Complete - load target scene
        SceneController.Direct(targetScene);
    }
    
    private void OnDestroy()
    {
        // Clean up DOTween
        DOTween.Kill(this);
    }
}