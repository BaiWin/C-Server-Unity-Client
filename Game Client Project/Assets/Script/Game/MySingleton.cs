using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MySingleton<T> where T : new()
{
    private static T _instance;
    private static object _lockObj = new object();
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
}
