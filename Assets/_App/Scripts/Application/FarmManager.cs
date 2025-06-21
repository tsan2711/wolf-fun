// FarmManager.cs - Đơn giản tạo plots trên farmContainer
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class FarmManager : MonoBehaviour
{
    [Header("Plot Settings")]
    public GameObject plotPrefab;
    public float plotSpacing = 3f;
    public int plotsPerRow = 5;
    public int maxPlotsPerZone = 16; // Số lượng tối đa plots trong mỗi zone

    [Header("Zone Layout Settings")]
    public float zoneSeparationX = 5f;  // Khoảng cách giữa zones theo X
    public float zoneSeparationZ = 5f;  // Khoảng cách giữa zones theo Z


    [Header("Plot Materials")]
    public Material emptyPlotMaterial;
    public Material growingPlotMaterial;
    public Material readyPlotMaterial;

    [Header("Content Prefabs")]
    public GameObject tomatoPrefab;
    public GameObject blueberryPrefab;
    public GameObject strawberryPrefab;
    public GameObject cowPrefab;

    [Header("Zone Colors")]
    public Material strawberryZoneMaterial;
    public Material tomatoZoneMaterial;
    public Material blueberryZoneMaterial;
    public Material cowZoneMaterial;

    private Dictionary<int, GameObject> plotGameObjects = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> contentGameObjects = new Dictionary<int, GameObject>();
    private Farm farm;

    private float uiUpdateTimer = 0f;
    private const float UI_UPDATE_INTERVAL = 1f; // Cập nhật mỗi giây


    private void Update()
    {
        if (farm == null) return;

        uiUpdateTimer += Time.deltaTime;
        if (uiUpdateTimer >= UI_UPDATE_INTERVAL)
        {
            uiUpdateTimer = 0f;
            UpdateAllPlotInfoDisplays(); // Cập nhật tất cả text displays
        }
    }

    private void UpdateAllPlotInfoDisplays()
    {
        foreach (var plot in farm.Plots)
        {
            if (plotGameObjects.TryGetValue(plot.Id, out GameObject plotGO))
            {
                UpdatePlotInfoDisplay(plotGO, plot);
            }
        }
    }

    public void Initialize(Farm farm)
    {
        this.farm = farm;
        CreateAllPlots();
        farm.FarmStateChanged += OnFarmStateChanged;
    }

    private void CreateAllPlots()
    {
        // Tạo plots cho tất cả plots trong farm
        foreach (var plot in farm.Plots)
        {
            CreatePlotGameObject(plot);
        }
    }

    private void CreatePlotGameObject(Plot plot)
    {
        if (plotPrefab == null)
        {
            Debug.LogError("Plot Prefab is null!");
            return;
        }

        Vector3 plotPosition = GetPlotWorldPosition(plot.Id);

        // Check collision
        if (IsPositionOccupied(plotPosition))
        {
            Debug.LogWarning($"Position {plotPosition} is occupied! Plot {plot.Id} may overlap.");
        }

        GameObject plotGO = Instantiate(plotPrefab, plotPosition, Quaternion.identity, transform);
        plotGO.name = $"Plot_{plot.Id}_{plot.Zone}";

        plotGameObjects[plot.Id] = plotGO;
        UpdatePlotVisual(plot);

        Debug.Log($"Created plot {plot.Id} at {plotPosition}");
    }
    public Vector3 GetPlotWorldPosition(int plotId)
    {
        var plot = farm.GetPlot(plotId);
        if (plot == null)
        {
            Debug.LogError($"FarmManager: Plot {plotId} not found!");
            return Vector3.zero;
        }

        var plotsInZone = farm.Plots
            .Where(p => p.Zone == plot.Zone)
            .OrderBy(p => p.Id)
            .ToList();

        int indexInZone = plotsInZone.FindIndex(p => p.Id == plotId);
        if (indexInZone < 0)
        {
            Debug.LogError($"FarmManager: Plot {plotId} not found in zone {plot.Zone}!");
            indexInZone = 0;
        }

        int row = indexInZone / plotsPerRow;
        int col = indexInZone % plotsPerRow;

        float localX = col * plotSpacing;
        float localZ = row * plotSpacing;

        Vector3 zoneOffset = GetZoneOffset(plot.Zone);
        Vector3 finalPosition = transform.position + zoneOffset + new Vector3(localX, 0, localZ);

        Debug.Log($"FarmManager GetPlotPosition: Plot {plotId} ({plot.Zone}) → " +
                 $"IndexInZone={indexInZone} (row:{row}, col:{col}) → Position={finalPosition}");

        return finalPosition;
    }
    private bool IsPositionOccupied(Vector3 position, float threshold = 1f)
    {
        foreach (var existingPlot in plotGameObjects.Values)
        {
            if (existingPlot != null)
            {
                float distance = Vector3.Distance(position, existingPlot.transform.position);
                if (distance < threshold)
                {
                    return true;
                }
            }
        }
        return false;
    }


    private int GetPlotIndexInZone(int plotId, PlotZone zone)
    {
        var plotsInZone = farm.Plots.Where(p => p.Zone == zone).ToList();
        return plotsInZone.FindIndex(p => p.Id == plotId);
    }
    private Vector3 GetZoneOffset(PlotZone zone)
    {
        return zone switch
        {
            PlotZone.Strawberry => new Vector3(0, 0, 0),
            PlotZone.Tomato => new Vector3(zoneSeparationX, 0, 0),
            PlotZone.Blueberry => new Vector3(0, 0, zoneSeparationZ),
            PlotZone.Cow => new Vector3(zoneSeparationX, 0, zoneSeparationZ),
            _ => Vector3.zero
        };
    }

    private void OnFarmStateChanged()
    {
        // Tạo plots mới nếu farm mở rộng
        foreach (var plot in farm.Plots)
        {
            if (!plotGameObjects.ContainsKey(plot.Id))
            {
                CreatePlotGameObject(plot);
            }
            else
            {
                UpdatePlotVisual(plot);
            }
        }
    }

    private void UpdatePlotVisual(Plot plot)
    {
        if (!plotGameObjects.TryGetValue(plot.Id, out GameObject plotGO)) return;

        Renderer renderer = plotGO.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Zone-based materials
            Material zoneMaterial = GetZoneMaterial(plot.Zone);

            if (plot.CanHarvest && readyPlotMaterial != null)
            {
                renderer.material = readyPlotMaterial;
            }
            else if (plot.Content != null && growingPlotMaterial != null)
            {
                renderer.material = growingPlotMaterial;
            }
            else if (zoneMaterial != null)
            {
                renderer.material = zoneMaterial; // Zone color when empty
            }
        }

        // Update content (crops/animals)
        UpdatePlotContent(plot);

        // Update info text
        UpdatePlotInfoDisplay(plotGO, plot);
    }

    private Material GetZoneMaterial(PlotZone zone)
    {
        return zone switch
        {
            PlotZone.Strawberry => strawberryZoneMaterial,
            PlotZone.Tomato => tomatoZoneMaterial,
            PlotZone.Blueberry => blueberryZoneMaterial,
            PlotZone.Cow => cowZoneMaterial,
            _ => emptyPlotMaterial
        };
    }

    private void UpdatePlotContent(Plot plot)
    {
        // Remove existing content
        if (contentGameObjects.TryGetValue(plot.Id, out GameObject existingContent))
        {
            if (existingContent != null)
            {
                DestroyImmediate(existingContent);
            }
            contentGameObjects.Remove(plot.Id);
        }

        // Add new content if plot has something
        if (plot.Content != null)
        {
            GameObject contentPrefab = GetContentPrefab(plot.Content);
            if (contentPrefab != null)
            {
                Vector3 plotPosition = GetPlotWorldPosition(plot.Id);
                Vector3 contentPosition = plotPosition + Vector3.up * 0.5f; // Spawn above plot
                GameObject contentGO = Instantiate(contentPrefab, contentPosition, Quaternion.identity, transform);
                contentGO.name = $"Content_{plot.Id}_{plot.Content.GetDisplayName()}";

                contentGameObjects[plot.Id] = contentGO;
            }
        }
    }

    private GameObject GetContentPrefab(IPlantable content)
    {
        return content switch
        {
            TomatoCrop => tomatoPrefab,
            BlueberryCrop => blueberryPrefab,
            StrawberryCrop => strawberryPrefab,
            Cow => cowPrefab,
            _ => null
        };
    }

    private void UpdatePlotInfoDisplay(GameObject plotGO, Plot plot)
    {
        // Find or create text display above plot
        Transform textTransform = plotGO.transform.Find("PlotInfo");
        if (textTransform == null)
        {
            GameObject textGO = new GameObject("PlotInfo");
            textGO.transform.SetParent(plotGO.transform);
            textGO.transform.localPosition = Vector3.up * 2f;

            // Create world space canvas
            Canvas canvas = textGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            textGO.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100); // Set size for text
            canvas.transform.localScale = Vector3.one * 0.01f;


            // Add text component
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Plot";
            text.font = Resources.Load<TMP_FontAsset>("Fonts/LilitaOne-(for 'greater than' symbol)");
            text.fontSize = 42;
            text.color = GetZoneColor(plot.Zone);
            text.alignment = TextAlignmentOptions.Center;

            // Add ContentSizeFitter
            textGO.AddComponent<ContentSizeFitter>();

            // Add FacingCamera script
            // textGO.AddComponent<FacingCamera>();

            textGO.transform.rotation = Quaternion.Euler(120, -180, 0);

            textGO.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 35, 0);


            textTransform = textGO.transform;
        }

        // Update text content
        var textComponent = textTransform.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            string plotInfo = $"Plot {plot.Id}\n";
            if (plot.Content != null)
            {
                plotInfo += $"{plot.Content.GetDisplayName()}\n";
                if (plot.CanHarvest)
                {
                    plotInfo += "Ready!";
                }
                else
                {
                    var timeToNext = plot.Content.GetTimeToNextHarvest();
                    plotInfo += $"{timeToNext.Minutes:D2}:{timeToNext.Seconds:D2}";
                }
                plotInfo += $"\n[{plot.Content.GetCurrentHarvests()}/{plot.Content.GetMaxHarvests()}]";
            }
            else
            {
                plotInfo += "Empty";
            }
            textComponent.text = plotInfo;
        }


    }

    // private 

    private Color GetZoneColor(PlotZone zone)
    {
        return zone switch
        {
            PlotZone.Strawberry => strawberryZoneMaterial.color,
            PlotZone.Tomato => tomatoZoneMaterial.color,
            PlotZone.Blueberry => blueberryZoneMaterial.color,
            PlotZone.Cow => cowZoneMaterial.color,
            _ => Color.white
        };
    }

    public GameObject GetContentGameObject(int plotId)
    {
        contentGameObjects.TryGetValue(plotId, out GameObject contentGO);
        return contentGO;
    }

    private void OnDestroy()
    {
        if (farm != null)
        {
            farm.FarmStateChanged -= OnFarmStateChanged;
        }

        // Clean up content objects
        foreach (var contentGO in contentGameObjects.Values)
        {
            if (contentGO != null)
                DestroyImmediate(contentGO);
        }
        contentGameObjects.Clear();
    }

    // public bool ReachMaxPlot()
    // {
    //     // Kiểm tra xem có đủ tiền để mở rộng farm không
    //     return  >= farm.GetExpansionCost();
    // }
}