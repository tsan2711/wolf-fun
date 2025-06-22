using System;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    [Header("Main Lighting")]
    [SerializeField] private Light mainLight; // Chỉ 1 light duy nhất

    [Header("Light Settings")]
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float minIntensity = 0.1f;

    // Simple color presets
    private readonly Color dawnColor = new Color(1f, 0.6f, 0.3f);
    private readonly Color dayColor = new Color(1f, 0.95f, 0.8f);
    private readonly Color duskColor = new Color(1f, 0.5f, 0.2f);
    private readonly Color nightColor = new Color(0.7f, 0.8f, 1f);

    private void Awake() => SetupLights();

    public void HandleHourUpdate(int hour)
    {
        // Đảm bảo hour trong range 0-23
        hour = Mathf.Clamp(hour, 0, 23);

        // Update light dựa trên hour
        UpdateMainLight(hour);

        // Update ambient lighting
        UpdateAmbientLight(hour);

        Debug.Log($"Hour {hour}: Light intensity {mainLight?.intensity:F1}");
    }

    private void SetupLights()
    {
        if (mainLight != null)
        {
            mainLight.type = LightType.Directional;
            mainLight.lightmapBakeType = LightmapBakeType.Mixed; // NOT Realtime
            mainLight.shadows = LightShadows.Soft;
        }
    }

    private void UpdateMainLight(int hour)
    {
        if (mainLight == null) return;

        // Simple intensity và color dựa trên hour
        float intensity;
        Color lightColor;

        if (hour >= 6 && hour <= 12)
        {
            // Morning: 6AM to 12PM
            float t = (hour - 6f) / 6f; // 0 to 1
            intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            lightColor = Color.Lerp(dawnColor, dayColor, t);
        }
        else if (hour > 12 && hour < 18)
        {
            // Afternoon: 12PM to 6PM
            float t = (hour - 12f) / 6f; // 0 to 1
            intensity = Mathf.Lerp(maxIntensity, minIntensity, t);
            lightColor = Color.Lerp(dayColor, duskColor, t);
        }
        else
        {
            // Night: 6PM to 6AM
            intensity = minIntensity;
            lightColor = nightColor;
        }

        // Apply settings
        mainLight.intensity = intensity;
        mainLight.color = lightColor;

        // FIX: Improved sun rotation - chiếu xuống từ trên cao
        float sunAngle = GetSunAngle(hour);
        mainLight.transform.rotation = Quaternion.Euler(sunAngle, 45f, 0f);
    }

    /// <summary>
    /// Tính góc chiếu sáng đúng hướng
    /// </summary>
    private float GetSunAngle(int hour)
    {
        if (hour >= 6 && hour <= 18)
        {
            // Day time: từ 30° (dawn) → 90° (noon) → 30° (dusk)
            if (hour <= 12)
            {
                // Morning: 6AM=30°, 12PM=90°
                return Mathf.Lerp(30f, 90f, (hour - 6f) / 6f);
            }
            else
            {
                // Afternoon: 12PM=90°, 6PM=30°
                return Mathf.Lerp(90f, 30f, (hour - 12f) / 6f);
            }
        }
        else
        {
            // Night time: góc thấp để tạo ánh sáng nhẹ
            return 15f;
        }
    }

    private void UpdateAmbientLight(int hour)
    {
        // Simple ambient lighting based on hour
        Color ambientColor;
        float ambientIntensity;

        if (hour >= 5 && hour <= 7)
        {
            // Dawn
            ambientColor = dawnColor;
            ambientIntensity = 0.4f;
        }
        else if (hour > 7 && hour < 17)
        {
            // Day
            ambientColor = dayColor;
            ambientIntensity = 0.8f;
        }
        else if (hour >= 17 && hour <= 19)
        {
            // Dusk
            ambientColor = duskColor;
            ambientIntensity = 0.4f;
        }
        else
        {
            // Night
            ambientColor = nightColor;
            ambientIntensity = 0.2f;
        }

        // Apply ambient settings
        RenderSettings.ambientSkyColor = ambientColor * ambientIntensity;
        RenderSettings.ambientEquatorColor = ambientColor * (ambientIntensity * 0.7f);
        RenderSettings.ambientGroundColor = ambientColor * (ambientIntensity * 0.3f);

        // Simple fog
        if (RenderSettings.fog)
        {
            RenderSettings.fogColor = ambientColor * 0.8f;
        }
    }

    [ContextMenu("Test Dawn (6AM)")]
    private void TestDawn() => HandleHourUpdate(6);
    [ContextMenu("Test Morning (10AM)")]
    private void TestMorning() => HandleHourUpdate(10);
    [ContextMenu("Test Noon (12PM)")]
    private void TestNoon() => HandleHourUpdate(12);
    [ContextMenu("Test Afternoon (3PM)")]
    private void TestAfternoon() => HandleHourUpdate(15);
    [ContextMenu("Test Dusk (6PM)")]
    private void TestDusk() => HandleHourUpdate(18);
    [ContextMenu("Test Night (10PM)")]
    private void TestNight() => HandleHourUpdate(22);
    [ContextMenu("Test Midnight (12AM)")]
    private void TestMidnight() => HandleHourUpdate(0);

}