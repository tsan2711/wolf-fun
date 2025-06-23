using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : Singleton<ObjectPooler>
{
    [Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 10;
    }
    public Pool[] pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;


    protected override void Awake()
    {
        base.Awake();
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Khởi tạo các pool
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Tạo trước một số object
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform); // Giữ hierarchy gọn gàng
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }


    public GameObject GetFromPool(string tag, Vector3 position)
    {
        return GetFromPool(tag, position, Quaternion.identity);
    }

    public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetFromPool(tag);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }

    public GameObject GetFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với tag '{tag}' không tồn tại!");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];

        // Nếu pool rỗng, tạo object mới
        if (pool.Count == 0)
        {
            Pool poolData = GetPoolByTag(tag);
            if (poolData != null)
            {
                GameObject newObj = Instantiate(poolData.prefab);
                newObj.transform.SetParent(transform);
                return newObj;
            }
            return null;
        }

        // Lấy object từ pool và kích hoạt
        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    private Pool GetPoolByTag(string tag)
    {
        foreach (Pool pool in pools)
        {
            if (pool.tag == tag)
                return pool;
        }
        return null;
    }

    public void Release(string tag, GameObject obj)
    {
        ReturnToPool(tag, obj);
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với tag '{tag}' không tồn tại!");
            return;
        }

        // Vô hiệu hóa object và trả về pool
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }


    private string GetPoolTagForContent(IPlantable content)
    {
        return content switch
        {
            TomatoCrop tomato => tomato.IsReadyToHarvest() ? Farm.TOMATOMATURE : Farm.TOMATOSEED,
            BlueberryCrop blueberry => blueberry.IsReadyToHarvest() ? Farm.BLUEBERRYMATURE : Farm.BLUEBERRYSEED,
            StrawberryCrop strawberry => strawberry.IsReadyToHarvest() ? Farm.STRAWBERRYMATURE : Farm.STRAWBERRYSEED,
            Cow cow => cow.IsReadyToHarvest() ? Farm.COWMATURE : Farm.COW,
            _ => "Unknown"
        };
    }

    public GameObject UpdateContentVisual(IPlantable content, GameObject currentObject)
    {
        if (content == null) return null;

        string requiredTag = GetPoolTagForContent(content);
        string currentTag = GetTagFromGameObject(currentObject);

        if (currentTag == requiredTag && currentObject != null)
        {
            return currentObject;
        }

        if (currentObject != null)
        {
            ReturnToPool(currentTag, currentObject);
        }

        return GetFromPool(requiredTag);
    }
    private string GetTagFromGameObject(GameObject obj)
    {
        if (obj == null) return string.Empty;

        // Kiểm tra tên object để xác định tag
        string objName = obj.name.Replace("(Clone)", "").Trim();

        foreach (Pool pool in pools)
        {
            if (objName.Contains(pool.prefab.name))
            {
                return pool.tag;
            }
        }

        return string.Empty;
    }

}
