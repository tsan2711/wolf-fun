using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollViewHelper : MonoBehaviour
{
    [Header("Auto Resize Settings")]
    [SerializeField] private bool autoResizeHeight = true;
    [SerializeField] private bool autoResizeWidth = false;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private float padding = 20f;

    private ScrollRect _scrollRect;
    private RectTransform _content;
    private RectTransform _viewport;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _content = _scrollRect.content;
        _viewport = _scrollRect.viewport;

        if (_content == null)
        {
            Debug.LogError("ScrollViewHelper: Content not found!");
            return;
        }

        // Auto-detect Layout Group if exists
        var layoutGroup = _content.GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            // Get spacing from existing layout group
            if (layoutGroup is VerticalLayoutGroup vlg)
                itemSpacing = vlg.spacing;
            else if (layoutGroup is HorizontalLayoutGroup hlg)
                itemSpacing = hlg.spacing;
            else if (layoutGroup is GridLayoutGroup glg)
                itemSpacing = glg.spacing.y;
        }
    }

    private void Start()
    {
        ResizeContent();
    }

    private void OnEnable()
    {
        // Listen for child changes
        if (_content != null)
        {
            // Force update on enable
            Invoke(nameof(ResizeContent), 0.1f);
        }
    }

    public void ResizeContent()
    {
        if (_content == null || _viewport == null) return;

        int childCount = GetActiveChildCount();
        
        if (childCount == 0)
        {
            SetContentSize(0, 0);
            return;
        }

        // Get first child to determine item size
        RectTransform firstChild = GetFirstActiveChild();
        if (firstChild == null) return;

        Vector2 itemSize = firstChild.sizeDelta;
        
        // Calculate new content size
        Vector2 newSize = _content.sizeDelta;

        if (autoResizeHeight)
        {
            float totalHeight = (itemSize.y * childCount) + (itemSpacing * (childCount - 1)) + (padding * 2);
            newSize.y = Mathf.Max(totalHeight, _viewport.rect.height);
        }

        if (autoResizeWidth)
        {
            float totalWidth = (itemSize.x * childCount) + (itemSpacing * (childCount - 1)) + (padding * 2);
            newSize.x = Mathf.Max(totalWidth, _viewport.rect.width);
        }

        SetContentSize(newSize.x, newSize.y);
    }

    private void SetContentSize(float width, float height)
    {
        if (_content == null) return;
        
        Vector2 newSize = new Vector2(width, height);
        if (_content.sizeDelta != newSize)
        {
            _content.sizeDelta = newSize;
        }
    }

    private int GetActiveChildCount()
    {
        if (_content == null) return 0;
        
        int count = 0;
        for (int i = 0; i < _content.childCount; i++)
        {
            if (_content.GetChild(i).gameObject.activeInHierarchy)
                count++;
        }
        return count;
    }

    private RectTransform GetFirstActiveChild()
    {
        if (_content == null) return null;
        
        for (int i = 0; i < _content.childCount; i++)
        {
            Transform child = _content.GetChild(i);
            if (child.gameObject.activeInHierarchy)
                return child as RectTransform;
        }
        return null;
    }

    // Public methods to call when items are added/removed
    public void OnItemAdded()
    {
        ResizeContent();
    }

    public void OnItemRemoved()
    {
        // Delay slightly to ensure the item is actually destroyed
        Invoke(nameof(ResizeContent), 0.05f);
    }

    public void OnItemsCleared()
    {
        ResizeContent();
    }

    // Force refresh - useful for external calls
    public void RefreshSize()
    {
        ResizeContent();
    }

    // Advanced: Auto-detect when children change (optional)
    private void Update()
    {
        // Only check every few frames for performance
        if (Time.frameCount % 10 == 0)
        {
            CheckForChildChanges();
        }
    }

    private int _lastChildCount = 0;
    private void CheckForChildChanges()
    {
        if (_content == null) return;
        
        int currentChildCount = GetActiveChildCount();
        if (currentChildCount != _lastChildCount)
        {
            _lastChildCount = currentChildCount;
            ResizeContent();
        }
    }
}