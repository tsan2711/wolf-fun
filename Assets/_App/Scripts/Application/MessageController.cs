using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class MessageSystem : Singleton<MessageSystem>
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button dismissButton; // Optional dedicated button

    [Header("Settings")]
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private float spamPreventTime = 1f;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float scaleInDuration = 0.25f;
    [SerializeField] private float slideDistance = 50f;
    [SerializeField] private AnimationType animationType = AnimationType.FadeScale;

    [Header("Interaction Settings")]
    [SerializeField] private bool clickToDismiss = true;
    [SerializeField] private float clickCooldown = 0.2f; // Prevent accidental immediate dismissal

    public enum AnimationType
    {
        Fade,
        Scale,
        FadeScale,
        SlideDown,
        SlideUp,
        Bounce
    }

    private Coroutine _showMessageCoroutine;
    private string _lastMessage = "";
    private float _lastMessageTime = 0f;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private bool _canDismiss = false;
    protected override bool IsDontDestroyOnLoad => false;

    protected override void Awake()
    {
        base.Awake();
        if (canvasGroup == null && messagePanel != null)
        {
            canvasGroup = messagePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = messagePanel.AddComponent<CanvasGroup>();
            }
        }

        if (messagePanel != null)
        {
            _originalScale = messagePanel.transform.localScale;
            _originalPosition = messagePanel.transform.localPosition;
            messagePanel.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        _canDismiss = false;

        // Setup click-to-dismis functionality
        SetupClickToDismiss();
    }

    public void ShowMessage(string message)
    {
        if (_lastMessage == message && Time.time - _lastMessageTime < spamPreventTime)
            return;

        _lastMessage = message;
        _lastMessageTime = Time.time;

        // Stop current message if showing
        if (_showMessageCoroutine != null)
        {
            StopCoroutine(_showMessageCoroutine);
        }

        // Kill any existing animations
        messagePanel?.transform.DOKill();
        canvasGroup?.DOKill();

        // Show new message immediately
        _showMessageCoroutine = StartCoroutine(ShowAnimatedMessageCoroutine(message));
    }

    private IEnumerator ShowAnimatedMessageCoroutine(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (messagePanel != null)
            messagePanel.SetActive(true);

        Debug.Log($"[MESSAGE] {message}");

        // Play entrance animation
        yield return StartCoroutine(PlayEntranceAnimation());

        // Enable dismiss after entrance animation + cooldown
        if (clickToDismiss)
        {
            yield return new WaitForSeconds(clickCooldown);
            _canDismiss = true;
        }

        // Wait for display time (but can be interrupted by click)
        float elapsedTime = 0f;
        while (elapsedTime < displayTime && _showMessageCoroutine != null)
        {
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        // Only continue if coroutine wasn't stopped by click
        if (_showMessageCoroutine != null)
        {
            // Play exit animation
            yield return StartCoroutine(PlayExitAnimation());

            if (messagePanel != null)
                messagePanel.SetActive(false);
        }

        _canDismiss = false;
        _showMessageCoroutine = null;
    }

    private IEnumerator PlayEntranceAnimation()
    {
        switch (animationType)
        {
            case AnimationType.Fade:
                yield return PlayFadeIn();
                break;
            case AnimationType.Scale:
                yield return PlayScaleIn();
                break;
            case AnimationType.FadeScale:
                yield return PlayFadeScaleIn();
                break;
            case AnimationType.SlideDown:
                yield return PlaySlideDown();
                break;
            case AnimationType.SlideUp:
                yield return PlaySlideUp();
                break;
            case AnimationType.Bounce:
                yield return PlayBounceIn();
                break;
        }
    }

    private IEnumerator PlayExitAnimation()
    {
        switch (animationType)
        {
            case AnimationType.Fade:
                yield return PlayFadeOut();
                break;
            case AnimationType.Scale:
                yield return PlayScaleOut();
                break;
            case AnimationType.FadeScale:
                yield return PlayFadeScaleOut();
                break;
            case AnimationType.SlideDown:
                yield return PlaySlideOutDown();
                break;
            case AnimationType.SlideUp:
                yield return PlaySlideOutUp();
                break;
            case AnimationType.Bounce:
                yield return PlayBounceOut();
                break;
        }
    }

    private void SetupClickToDismiss()
    {
        if (!clickToDismiss) return;

        // Setup dedicated dismiss button if assigned
        if (dismissButton != null)
        {
            dismissButton.onClick.AddListener(DismissMessage);
        }

        // Setup click-to-dismiss on the panel itself
        if (messagePanel != null)
        {
            // Add Button component if not present
            Button panelButton = messagePanel.GetComponent<Button>();
            if (panelButton == null)
            {
                panelButton = messagePanel.AddComponent<Button>();
                // Make it invisible but clickable
                panelButton.transition = Selectable.Transition.None;
            }

            panelButton.onClick.RemoveAllListeners();
            panelButton.onClick.AddListener(DismissMessage);
        }
    }

    private void DismissMessage()
    {
        if (!_canDismiss || _showMessageCoroutine == null) return;

        Debug.Log("[MESSAGE] Dismissed by click");

        // Stop the current coroutine and start exit animation
        StopCoroutine(_showMessageCoroutine);
        _canDismiss = false;

        StartCoroutine(DismissMessageCoroutine());
    }

    private IEnumerator DismissMessageCoroutine()
    {
        // Play exit animation
        yield return StartCoroutine(PlayExitAnimation());

        if (messagePanel != null)
            messagePanel.SetActive(false);

        _showMessageCoroutine = null;
    }

    private IEnumerator PlayFadeIn()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            yield return canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeInDuration);
        }
    }

    private IEnumerator PlayScaleIn()
    {
        if (messagePanel != null)
        {
            messagePanel.transform.localScale = Vector3.zero;
            yield return messagePanel.transform.DOScale(_originalScale, scaleInDuration).SetEase(Ease.OutBack).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(scaleInDuration);
        }
    }

    private IEnumerator PlayFadeScaleIn()
    {
        if (messagePanel != null && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            messagePanel.transform.localScale = Vector3.zero;

            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
            sequence.Join(messagePanel.transform.DOScale(_originalScale, scaleInDuration).SetEase(Ease.OutBack));

            yield return sequence.WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(Mathf.Max(fadeInDuration, scaleInDuration));
        }
    }

    private IEnumerator PlaySlideDown()
    {
        if (messagePanel != null)
        {
            Vector3 startPos = _originalPosition + Vector3.up * slideDistance;
            messagePanel.transform.localPosition = startPos;

            if (canvasGroup != null) canvasGroup.alpha = 1f;

            yield return messagePanel.transform.DOLocalMove(_originalPosition, fadeInDuration).SetEase(Ease.OutQuart).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeInDuration);
        }
    }

    private IEnumerator PlaySlideUp()
    {
        if (messagePanel != null)
        {
            Vector3 startPos = _originalPosition + Vector3.down * slideDistance;
            messagePanel.transform.localPosition = startPos;

            if (canvasGroup != null) canvasGroup.alpha = 1f;

            yield return messagePanel.transform.DOLocalMove(_originalPosition, fadeInDuration).SetEase(Ease.OutQuart).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeInDuration);
        }
    }

    private IEnumerator PlayBounceIn()
    {
        if (messagePanel != null && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            messagePanel.transform.localScale = Vector3.zero;

            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, fadeInDuration * 0.5f).SetEase(Ease.OutQuad));
            sequence.Join(messagePanel.transform.DOScale(_originalScale * 1.1f, scaleInDuration * 0.6f).SetEase(Ease.OutQuad));
            sequence.Append(messagePanel.transform.DOScale(_originalScale, scaleInDuration * 0.4f).SetEase(Ease.OutBounce));

            yield return sequence.WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(scaleInDuration);
        }
    }

    private IEnumerator PlayFadeOut()
    {
        if (canvasGroup != null)
        {
            yield return canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
    }

    private IEnumerator PlayScaleOut()
    {
        if (messagePanel != null)
        {
            yield return messagePanel.transform.DOScale(Vector3.zero, fadeOutDuration).SetEase(Ease.InBack).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
    }

    private IEnumerator PlayFadeScaleOut()
    {
        if (messagePanel != null && canvasGroup != null)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
            sequence.Join(messagePanel.transform.DOScale(Vector3.zero, fadeOutDuration).SetEase(Ease.InBack));

            yield return sequence.WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
    }

    private IEnumerator PlaySlideOutDown()
    {
        if (messagePanel != null)
        {
            Vector3 endPos = _originalPosition + Vector3.down * slideDistance;
            yield return messagePanel.transform.DOLocalMove(endPos, fadeOutDuration).SetEase(Ease.InQuart).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
    }

    private IEnumerator PlaySlideOutUp()
    {
        if (messagePanel != null)
        {
            Vector3 endPos = _originalPosition + Vector3.up * slideDistance;
            yield return messagePanel.transform.DOLocalMove(endPos, fadeOutDuration).SetEase(Ease.InQuart).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
    }

    private IEnumerator PlayBounceOut()
    {
        if (messagePanel != null && canvasGroup != null)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(messagePanel.transform.DOScale(_originalScale * 1.05f, fadeOutDuration * 0.3f).SetEase(Ease.OutQuad));
            sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration * 0.7f).SetEase(Ease.InQuad));
            sequence.Join(messagePanel.transform.DOScale(Vector3.zero, fadeOutDuration * 0.7f).SetEase(Ease.InBack));

            yield return sequence.WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
    }

    public void ClearMessage()
    {
        if (_showMessageCoroutine != null)
        {
            StopCoroutine(_showMessageCoroutine);
            _showMessageCoroutine = null;
        }

        // Kill animations and hide immediately
        messagePanel?.transform.DOKill();
        canvasGroup?.DOKill();

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
            messagePanel.transform.localScale = _originalScale;
            messagePanel.transform.localPosition = _originalPosition;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    public void ClearQueue()
    {
        ClearMessage();
    }

    public void SetAnimationType(AnimationType newType)
    {
        animationType = newType;
    }

    public void SetClickToDismiss(bool enabled)
    {
        clickToDismiss = enabled;
        if (enabled)
        {
            SetupClickToDismiss();
        }
    }

    public void ShowMessageWithAnimation(string message, AnimationType specificAnimation)
    {
        AnimationType oldType = animationType;
        animationType = specificAnimation;
        ShowMessage(message);
        animationType = oldType;
    }

    protected override void OnDestroy()
    {
        messagePanel?.transform.DOKill();
        canvasGroup?.DOKill();
    }
}