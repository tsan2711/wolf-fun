using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _progressText; // Text to display the progress percentage
    [SerializeField] private Image _fillImage;
    [SerializeField] private Color _initProgressColor = Color.gray; // Initial color when progress is zero
    [SerializeField] private Color _lowProgressColor = Color.red;
    [SerializeField] private Color _highProgressColor = Color.green;
    [SerializeField] private float _lowProgressThreshold = 0.3f; // Threshold for low progress color
    [SerializeField] private float _highProgressThreshold = 0.7f; // Threshold for high progress color


    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        _fillImage.fillAmount = progress;

        if (_progressText != null)
        {
            _progressText.text = $"{progress * 100f:0.0}%"; // Display progress as percentage
        }

        if (progress <= _lowProgressThreshold)
        {
            _fillImage.color = _lowProgressColor;
        }
        else if (progress >= _highProgressThreshold)
        {
            _fillImage.color = _highProgressColor;
        }
        else
        {
            float t = (progress - _lowProgressThreshold) / (_highProgressThreshold - _lowProgressThreshold);
            _fillImage.color = Color.Lerp(_lowProgressColor, _highProgressColor, t);
        }
    }

    public void ResetProgress()
    {
        if(_fillImage == null || _progressText == null)
        {
            return;
        }
        _progressText.text = "0%"; // Reset text to 0%
        _fillImage.fillAmount = 0f;
        _fillImage.color = _initProgressColor; // Reset to initial color
    }
    public void Deactivate() => gameObject.SetActive(false);
    public void Activate() => gameObject.SetActive(true);

}
