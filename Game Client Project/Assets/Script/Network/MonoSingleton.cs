using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour
       where T : MonoBehaviour
{
    private static bool applicationIsQuitting = false;
    private static T _instance;
    private static object _lockObj = new object();
    public static T Instance
    {
        get
        {
            if (applicationIsQuitting) { return _instance; }

            if (_instance != null)
            {
                return _instance;
            }
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
            }
            if (_instance == null)
            {
                lock (_lockObj)
                {
                    GameObject go = new GameObject("[MonoSingleton]" + typeof(T).Name);
                    _instance = go.AddComponent<T>();
                    DontDestroyOnLoad(go);
                }
            }
            else
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    public virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }
}
