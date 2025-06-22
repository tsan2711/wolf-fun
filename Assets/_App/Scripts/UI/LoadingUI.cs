using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

public class LoadingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Settings")]
    [SerializeField] private float loadingTime = 2f;
    [SerializeField] private float dotAnimationSpeed = 0.5f;

    private string targetScene;
    private string baseText = "Loading";

    private CancellationTokenSource _cancellationTokenSource;

    private void Start()
    {
        // Get target scene
        targetScene = PlayerPrefs.GetString("TargetScene", "GameScene");
        PlayerPrefs.DeleteKey("TargetScene");

        GetCancellationToken();
        StartLoading();
    }

    private void GetCancellationToken()
    {
        if (_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();
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
        LoadingProcessAsync().Forget();
    }

    private void StartTextAnimation() => AnimateLoadingDotsAsync().Forget();


    private async UniTaskVoid AnimateLoadingDotsAsync()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount + 1) % 4; // 0, 1, 2, 3, then back to 0

            string dots = new string('.', dotCount);
            if (loadingText != null)
                loadingText.text = baseText + dots;

            await UniTask.Delay(TimeSpan.FromSeconds(dotAnimationSpeed), cancellationToken: _cancellationTokenSource.Token);
        }
    }

    private async UniTaskVoid LoadingProcessAsync()
    {
        float elapsed = 0f;

        // Wait for loading time
        while (elapsed < loadingTime)
        {
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        // Complete - load target scene
        SceneController.Direct(targetScene);
    }

    private void OnDestroy()
    {
        // Clean up DOTween
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        DOTween.Kill(this);
    }
}