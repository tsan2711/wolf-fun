using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading;
using Cysharp.Threading.Tasks;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private AnimatedButton newGameButton;
    [SerializeField] private AnimatedButton continueButton;
    [SerializeField] private AnimatedButton quitButton;
    [SerializeField] private AnimatedImage logo;

    [Header("Animation Timing")]
    [SerializeField] private float logoDelay = 0.5f;
    [SerializeField] private float buttonDelay = 1.5f;
    [SerializeField] private float buttonInterval = 0.3f;

    private CancellationTokenSource _cancellationToken;

    private void Start()
    {
        SetupButtons();
        StartAnimations();
    }

    private void SetupButtons()
    {
        // Setup button actions
        if (newGameButton != null)
            newGameButton.GetComponent<Button>().onClick.AddListener(NewGame);

        if (continueButton != null)
        {
            continueButton.GetComponent<Button>().onClick.AddListener(ContinueGame);
            continueButton.GetComponent<Button>().interactable = SaveLoadSystem.HasSaveFile();
        }

        if (quitButton != null)
            quitButton.GetComponent<Button>().onClick.AddListener(QuitGame);
    }

    private void StartAnimations()
    {
        GetCancellationToken();
        // Animate logo
        if (logo != null)
            logo.AnimateIn(logoDelay);

        // Animate buttons

        AnimateButtonsAsync().Forget();
    }

    private async UniTaskVoid AnimateButtonsAsync()
    {
        await UniTask.Delay((int)buttonDelay * 1000, cancellationToken: _cancellationToken.Token);

        var buttons = new AnimatedButton[] { newGameButton, continueButton, quitButton };

        foreach (var button in buttons)
        {
            if (button != null)
            {
                button.AnimateIn();
                await UniTask.Delay((int)(buttonInterval * 1000));
            }
        }
    }

    private void GetCancellationToken()
    {
        if (_cancellationToken == default)
            _cancellationToken = new CancellationTokenSource();
    }


    private void NewGame()
    {
        // Transition effect
        transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            SaveLoadSystem.DeleteSave();
            SceneController.Loading(); // Go to game via loading
        });

        SetContinue(0); // Reset continue state
    }

    private void ContinueGame()
    {
        if (SaveLoadSystem.HasSaveFile())
        {
            transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                SceneController.Game(); // Go to game via loading
            });
        }

        SetContinue(1);
    }


    private void SetContinue(int value)
    {
        PlayerPrefs.SetInt("ContinueGame", value);
    }

    private void QuitGame()
    {
        GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(() =>
        {
            SceneController.Exit();
        });
    }

    private void OnDestroy()
    {
        _cancellationToken?.Cancel();
        _cancellationToken?.Dispose();
    }

}