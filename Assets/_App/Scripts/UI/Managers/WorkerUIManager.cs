using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;

public class WorkerUIManager : MonoBehaviour, IUIManager
{
    [Header("Prefab References")]
    [SerializeField] private WorkerUIItem workerItemPrefab;

    [Header("Container References")]
    public Transform WorkerContainer;
    [SerializeField] private Transform workersContent;

    [Header("Icon References")]
    [SerializeField] private InventoryIconData iconData;

    [Header("Button References")]
    [SerializeField] private Button hireWorkerButton;

    private Dictionary<int, WorkerUIItem> _workerItems = new Dictionary<int, WorkerUIItem>();

    public Action OnHireWorkerRequested;

    public void Initialize()
    {
        ValidateEvents();
    }

    private void ValidateEvents()
    {
        hireWorkerButton.onClick.AddListener(() => OnHireWorkerRequested?.Invoke());
    }

    public void UpdateWorkers(List<Worker> workers)
    {
        // Remove workers that no longer exist
        var existingWorkerIds = new HashSet<int>(workers.Select(w => w.Id));
        var workersToRemove = _workerItems.Keys.Where(id => !existingWorkerIds.Contains(id)).ToList();

        foreach (var workerId in workersToRemove)
        {
            if (_workerItems.ContainsKey(workerId))
            {
                Destroy(_workerItems[workerId].gameObject);
                _workerItems.Remove(workerId);
            }
        }

        // Add new workers or update existing ones
        foreach (var worker in workers)
        {
            if (!_workerItems.ContainsKey(worker.Id))
            {
                GameObject itemGO = Instantiate(workerItemPrefab.gameObject, workersContent);
                WorkerUIItem item = itemGO.GetComponent<WorkerUIItem>();

                Sprite icon = iconData.GetWorkerIcon();
                item.Initialize(worker, icon); // Use new overload with full worker info
                _workerItems[worker.Id] = item;
            }
            else
            {
                // Update existing worker UI (in case stats changed)
                Sprite icon = iconData.GetWorkerIcon();
                _workerItems[worker.Id].Initialize(worker, icon);
            }
        }
    }

    public void Activate(bool v)
    {
        WorkerContainer.gameObject.SetActive(v);
    }
}