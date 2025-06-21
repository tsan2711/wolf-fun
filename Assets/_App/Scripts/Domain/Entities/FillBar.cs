using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _progressText; 
    [SerializeField] private Image _fillImage;
    [Header("Progress Colors")]
    [SerializeField] private Color _initProgressColor = Color.gray; 
    [SerializeField] private Color _lowProgressColor = Color.red;
    [SerializeField] private Color _midProgressColor = Color.yellow; 
    [SerializeField] private Color _highProgressColor = Color.green;
    [SerializeField] private Color _initProgressTextColor = Color.white; 
    [SerializeField] private Color _lowProgressTextColor = Color.red; 
    [SerializeField] private Color _midProgressTextColor = Color.yellow; 
    [SerializeField] private Color _highProgressTextColor = Color.green; 
    [Header("Progress Thresholds")]
    [SerializeField] private float _lowProgressThreshold = 0.25f; 
    [SerializeField] private float _midProgressThreshold = 0.5f; 
    [SerializeField] private float _highProgressThreshold = 0.75f; 

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        _fillImage.fillAmount = progress;

        if (_progressText != null)
        {
            _progressText.text = $"{progress * 100f:0.0}%"; 
        }

        if (_fillImage == null) return;

        if (progress < _lowProgressThreshold)
        {
            _progressText.color = _lowProgressTextColor; 
            _fillImage.color = _lowProgressColor; 
        }
        else if (progress < _midProgressThreshold)
        {
            _progressText.color = Color.Lerp(_lowProgressTextColor, _midProgressTextColor, (progress - _lowProgressThreshold) / (_midProgressThreshold - _lowProgressThreshold));
            _fillImage.color = Color.Lerp(_initProgressColor, _midProgressColor, (progress - _lowProgressThreshold) / (_midProgressThreshold - _lowProgressThreshold));
        }
        else if (progress < _highProgressThreshold)
        {
            _progressText.color = Color.Lerp(_midProgressTextColor, _highProgressTextColor, (progress - _midProgressThreshold) / (_highProgressThreshold - _midProgressThreshold));
            _fillImage.color = Color.Lerp(_midProgressColor, _highProgressColor, (progress - _midProgressThreshold) / (_highProgressThreshold - _midProgressThreshold));
        }
        else
        {
            _progressText.color = _highProgressTextColor; 
            _fillImage.color = _highProgressColor; 
        }
    }

    public void ResetProgress()
    {
        if (_fillImage == null || _progressText == null)
        {
            return;
        }
        _progressText.text = "0%"; 
        _progressText.color = _initProgressTextColor; 
        _fillImage.fillAmount = 0f;
        _fillImage.color = _initProgressColor; 
    }
    public void Deactivate() => gameObject.SetActive(false);
    public void Activate() => gameObject.SetActive(true);

}
