// FarmManager.cs - Đơn giản tạo plots trên farmContainer
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Text;

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


    private StringBuilder _plotInfoBuilder = new StringBuilder(100);



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
        farm.PlotStateChanged += OnPlotStateChanged;
    }

    private void OnPlotStateChanged(Plot plot)
    {
        // Cập nhật visual cho plot khi trạng thái thay đổi
        if (plotGameObjects.TryGetValue(plot.Id, out GameObject plotGO))
        {
            UpdatePlotVisual(plot);
        }
        else
        {
            Debug.LogError($"FarmManager: Plot GameObject for plot {plot.Id} not found!");
        }
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
        if (!plotGameObjects.TryGetValue(plot.Id, out GameObject plotGO))
        {
            Debug.LogError($"FarmManager: Plot GameObject for plot {plot.Id} not found!");
            return;
        }

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

        Debug.Log("FarmManager: Updating content for plot " + plot.Id);

        GameObject currentContent = null;
        contentGameObjects.TryGetValue(plot.Id, out currentContent);

        if (plot.Content == null)
        {
            Debug.Log("FarmManager: Plot " + plot.Id + " is empty, removing content.");
            if (currentContent != null)
            {
                Debug.Log("FarmManager: Returning content to pool for plot " + plot.Id);
                string currentTag = GetContentTag(currentContent);

                Debug.Log("FarmManager: Returning content with tag: " + currentTag);
                ObjectPooler.Instance.ReturnToPool(currentTag, currentContent);
                contentGameObjects.Remove(plot.Id);
            }
            return;
        }


        GameObject newContent = ObjectPooler.Instance.UpdateContentVisual(plot.Content, currentContent);
        
        if (newContent != null)
        {
            Vector3 plotPosition = GetPlotWorldPosition(plot.Id);
            Vector3 contentPosition = plotPosition + Vector3.up * 0.5f;

            newContent.transform.position = contentPosition;
            newContent.transform.SetParent(transform);
            newContent.name = $"{plot.Content.GetDisplayName()}";

            // Cập nhật dictionary
            contentGameObjects[plot.Id] = newContent;
        }
    }

    private string GetContentTag(GameObject contentObj)
    {
        if (contentObj == null) return string.Empty;

        string objName = contentObj.name.Replace("(Clone)", "").Trim();

        Debug.Log("FarmManager: Getting tag for content object: " + objName);

        // Kiểm tra tên để xác định tag
        if (objName.Contains("Tomato Seed")) return Farm.TOMATOSEED;
        if (objName.Contains("Tomato Mature")) return Farm.TOMATOMATURE;
        if (objName.Contains("Blueberry Seed")) return Farm.BLUEBERRYSEED;
        if (objName.Contains("Blueberry Mature")) return Farm.BLUEBERRYMATURE;
        if (objName.Contains("Strawberry Seed")) return Farm.STRAWBERRYSEED;
        if (objName.Contains("Strawberry Mature")) return Farm.STRAWBERRYMATURE;
        if (objName.Contains("Cow") && !objName.Contains("Mature")) return Farm.COW;
        if (objName.Contains("Cow Mature")) return Farm.COWMATURE;

        return string.Empty;
    }

    private GameObject GetContentPrefab(IPlantable content)
    {
        return content switch
        {
            TomatoCrop => ObjectPooler.Instance.GetFromPool(Farm.TOMATOSEED),
            BlueberryCrop => ObjectPooler.Instance.GetFromPool(Farm.BLUEBERRYSEED),
            StrawberryCrop => ObjectPooler.Instance.GetFromPool(Farm.STRAWBERRYSEED),
            Cow => ObjectPooler.Instance.GetFromPool(Farm.COW),
            _ => null
        };
    }


    private void UpdatePlotInfoDisplay(GameObject plotGO, Plot plot)
    {
        Transform textTransform = plotGO.transform.Find("PlotInfo");
        if (textTransform == null)
        {
            GameObject textGO = new GameObject("PlotInfo");
            textGO.transform.SetParent(plotGO.transform);
            textGO.transform.localPosition = Vector3.up * 2f;

            Canvas canvas = textGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            textGO.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100); // Set size for text
            canvas.transform.localScale = Vector3.one * 0.01f;

            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Plot";
            text.font = Resources.Load<TMP_FontAsset>("Fonts/LilitaOne-(for 'greater than' symbol)");
            text.fontSize = 42;
            text.color = GetZoneColor(plot.Zone);
            text.alignment = TextAlignmentOptions.Center;

            textGO.AddComponent<ContentSizeFitter>();

            textGO.transform.rotation = Quaternion.Euler(120, -180, 0);
            textGO.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 35, 0);

            textTransform = textGO.transform;
        }

        var textComponent = textTransform.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            _plotInfoBuilder.Clear();
            _plotInfoBuilder.Append("Plot ").Append(plot.Id).Append('\n');

            if (plot.Content != null)
            {
                _plotInfoBuilder.Append(plot.Content.GetDisplayName()).Append('\n');

                if (plot.CanHarvest)
                {
                    // if (plot.IsTriggerUpdateVisual == false)
                    // {
                    //     plot.IsTriggerUpdateVisual = true;
                    //     OnPlotStateChanged(plot);
                    // }
                    _plotInfoBuilder.Append("Ready!");
                }
                else
                {
                    var timeToNext = plot.Content.GetTimeToNextHarvest();
                    _plotInfoBuilder.Append(timeToNext.Minutes.ToString("D2"))
                                   .Append(':')
                                   .Append(timeToNext.Seconds.ToString("D2"));
                }

                _plotInfoBuilder.Append('\n')
                               .Append('[')
                               .Append(plot.Content.GetCurrentHarvests())
                               .Append('/')
                               .Append(plot.Content.GetMaxHarvests())
                               .Append(']');
            }
            else
            {
                _plotInfoBuilder.Append("Empty");
            }

            textComponent.text = _plotInfoBuilder.ToString();
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


    public void CleanupForNewGame()
    {
        Debug.Log("FarmManager: Cleaning up for new game");

        if (farm != null)
        {
            farm.FarmStateChanged -= OnFarmStateChanged;
        }

        foreach (var contentGO in contentGameObjects.Values)
        {
            if (contentGO != null)
                DestroyImmediate(contentGO);
        }
        contentGameObjects.Clear();

        foreach (var plotGO in plotGameObjects.Values)
        {
            if (plotGO != null)
                DestroyImmediate(plotGO);
        }
        plotGameObjects.Clear();

        farm = null;

        Debug.Log("FarmManager cleanup complete");
    }
}