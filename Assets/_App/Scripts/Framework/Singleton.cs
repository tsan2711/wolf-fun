using System;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static Dictionary<Type, MonoBehaviour> instances = new Dictionary<Type, MonoBehaviour>();

    protected virtual bool IsDontDestroyOnLoad => true;
    public static T Instance
    {
        get
        {
            Type type = typeof(T);

            if (instances.ContainsKey(type))
            {
                return instances[type] as T;
            }

            T foundInstance = FindAnyObjectByType<T>();
            if (foundInstance == null)
            {
                Debug.LogWarning($"No instance of Singleton {type} found.");
                return null;
            }

            (foundInstance as Singleton<T>)?.RegisterInstance();
            return foundInstance;
        }
    }

    protected virtual void Awake() => RegisterInstance();

    private void RegisterInstance()
    {
        Type type = typeof(T);

        if (!instances.ContainsKey(type))
        {
            instances[type] = this;

            if (IsDontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }
        else if (instances[type] != this)
        {
            Debug.LogWarning($"Multiple instances of Singleton {type} found. Destroying extra instance on {gameObject.name}.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        Type type = typeof(T);
        if (instances.ContainsKey(type) && instances[type] == this)
        {
            instances.Remove(type);
        }
    }
}