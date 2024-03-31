using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class PoolItem
{
    public GameObject Prefab;
    public int PoolCount = PoolManager.DEFAULT_POOL_COUNT;

    public PoolItem(GameObject prefab)
    {
        Prefab = prefab;
    }

    public PoolItem(GameObject prefab, int poolCount)
    {
        Prefab = prefab;
        PoolCount = poolCount;
    }
}
