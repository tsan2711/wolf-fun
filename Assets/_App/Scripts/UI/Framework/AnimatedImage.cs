using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class AnimatedImage : MonoBehaviour
{
  [Header("Animation Settings")]
    [SerializeField] private AnimationType entranceType = AnimationType.ScaleUp;
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private bool floatingEffect = true;
    
    private Vector3 originalScale;
    private Vector2 originalPosition;
    
    public enum AnimationType
    {
        ScaleUp,
        FadeIn,
        Bounce
    }
    
    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = GetComponent<RectTransform>().anchoredPosition;
    }
    
    private void Start()
    {
        SetInitialState();
    }
    
    private void SetInitialState()
    {
        switch (entranceType)
        {
            case AnimationType.ScaleUp:
                transform.localScale = Vector3.zero;
                break;
            case AnimationType.FadeIn:
                var image = GetComponent<Image>();
                if (image != null)
                {
                    Color color = image.color;
                    color.a = 0f;
                    image.color = color;
                }
                break;
            case AnimationType.Bounce:
                transform.localScale = Vector3.zero;
                break;
        }
    }
    
    public void AnimateIn(float delay = 0f)
    {
        switch (entranceType)
        {
            case AnimationType.ScaleUp:
                transform.DOScale(originalScale, animationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack)
                    .OnComplete(StartFloating);
                break;
                
            case AnimationType.FadeIn:
                var image = GetComponent<Image>();
                if (image != null)
                {
                    image.DOFade(1f, animationDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(StartFloating);
                }
                break;
                
            case AnimationType.Bounce:
                transform.DOScale(originalScale, animationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBounce)
                    .OnComplete(StartFloating);
                break;
        }
    }
    
    private void StartFloating()
    {
        if (floatingEffect)
        {
            GetComponent<RectTransform>().DOAnchorPosY(originalPosition.y + 20f, 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    private void OnDestroy()
    {
        transform.DOKill();
    }
}