using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedButton : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private AnimationType entranceType = AnimationType.ScaleUp;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float hoverScale = 1.1f;

    private Button button;
    private Vector3 originalScale;
    private bool isInitialized = false;

    public enum AnimationType
    {
        ScaleUp,
        SlideFromBottom,
        FadeIn
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
        SetupHoverEffects();
    }

    private void Start()
    {
        SetInitialState();
        isInitialized = true;
    }

    private void SetupHoverEffects()
    {
        if (button == null) return;

        var eventTrigger = gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // Hover enter
        var hoverEntry = new UnityEngine.EventSystems.EventTrigger.TriggerEvent();
        hoverEntry.AddListener((data) => OnHoverEnter());
        var hoverTrigger = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter,
            callback = hoverEntry
        };
        eventTrigger.triggers.Add(hoverTrigger);

        // Hover exit
        var exitEntry = new UnityEngine.EventSystems.EventTrigger.TriggerEvent();
        exitEntry.AddListener((data) => OnHoverExit());
        var exitTrigger = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit,
            callback = exitEntry
        };
        eventTrigger.triggers.Add(exitTrigger);
    }

    private void SetInitialState()
    {
        switch (entranceType)
        {
            case AnimationType.ScaleUp:
                transform.localScale = Vector3.zero;
                break;
            case AnimationType.SlideFromBottom:
                GetComponent<RectTransform>().anchoredPosition += Vector2.down * 300f;
                break;
            case AnimationType.FadeIn:
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
                break;
        }
    }

    public void AnimateIn(float delay = 0f)
    {
        if (!isInitialized) return;

        switch (entranceType)
        {
            case AnimationType.ScaleUp:
                transform.DOScale(originalScale, animationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack);
                break;

            case AnimationType.SlideFromBottom:
                GetComponent<RectTransform>().DOAnchorPosY(
                    GetComponent<RectTransform>().anchoredPosition.y + 300f,
                    animationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutQuad);
                break;

            case AnimationType.FadeIn:
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(1f, animationDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.OutQuad);
                }
                break;
        }
    }

    private void OnHoverEnter()
    {
        if (button != null && button.interactable)
        {
            transform.DOScale(originalScale * hoverScale, 0.2f)
                .SetEase(Ease.OutQuad);
        }
    }

    private void OnHoverExit()
    {
        if (button != null && button.interactable)
        {
            transform.DOScale(originalScale, 0.2f)
                .SetEase(Ease.OutQuad);
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
