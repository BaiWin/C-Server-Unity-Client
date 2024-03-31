using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoSingleton<PoolManager>
{
    // Default pool size for each pool item
    public static int DEFAULT_POOL_COUNT = 1;

    // Data Setup
    [SerializeField] private List<PoolItem> poolableObjects = new List<PoolItem>();

    // Collection checks will throw errors if we try to release an item that is already in the pool.
    [SerializeField] private bool collectionChecks = true;

    // Maximum collection size allocated per pooled object
    [SerializeField] private int maxPoolSize = 10;

    private Dictionary<string, IObjectPool<PooledGameObject>> pool;

    public Dictionary<string, IObjectPool<PooledGameObject>> Pool => pool;

    //Check if pool manager is ready for use
    public bool IsReady { get; private set; }

    //Callback when the pool manager is going to be destroyed. Pooled GameObjects should be returned to the pool and destroyed as well
    public Action OnCleanup;
    //Callback when pool manager is going to release all pooled GameObjects. The GameObjects do not get destroyed at this point, they are just returned to the pool and get disabled
    public Action OnRelease;

    public void InstantiatePool()
    {
        if (IsReady)
        {
            return;
        }

        pool = DictionaryPool<string, IObjectPool<PooledGameObject>>.Get();

        // Memory allocation and initialize GameObject
        poolableObjects.ForEach(
            item =>
            {
                // Create stack in dictionary
                CreateObjectsPool(item);
                InitializeObjectsPool(item, item.PoolCount);
            }
        );

        OnRelease?.Invoke();
        IsReady = true;

    }

    public void CleanupPool(bool garbageCollect = true)
    {
        if (!IsReady)
        {
            return;
        }

        // Destroy all game objects and release allocated memory
        OnCleanup?.Invoke();
        DictionaryPool<string, IObjectPool<PooledGameObject>>.Release(pool);

        IsReady = false;
        OnCleanup = null;

        if(garbageCollect)
        {
            GC.Collect();
        }
    }

    public void ReleasePooledObject()
    {
        if (!IsReady)
        {
            return;
        }

        OnRelease?.Invoke();
    }

    private void CreateObjectsPool(PoolItem item)
    {
        if (!item.Prefab)
        {
            throw new Exception("Pool Item prefab is null");
        }

        var id = item.Prefab.name;

        // Game object doesn't get created at this point, we need to call pool.Get() to do so
        pool.Add(
            id,
            new ObjectPool<PooledGameObject>(
                () => CreatePooledItem(item.Prefab),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPoolObject,
                collectionChecks,
                item.PoolCount,
                maxPoolSize
            )
        );
    }

    private void InitializeObjectsPool(PoolItem item, int count)
    {
        if (!item.Prefab)
        {
            throw new Exception("Pool Item prefab is null");
        }

        var id = item.Prefab.name;
        if (!pool.TryGetValue(id, out var objectPool))
        {
            CreateObjectsPool(item);
            objectPool = pool[id];
        }

        // Object shouldn't be release at this point else it will just take the same item from the top of the stack
        for (var i = 0; i < count; ++i)
        {
            objectPool.Get();
        }
    }

    //Actual GameObject creation. Here we¡¯re adding another component PooledGameObject which be use for lifetime management.
    private PooledGameObject CreatePooledItem(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        go.name = prefab.name;

        var pooledGO = go.AddComponent<PooledGameObject>();
        pooledGO.Initialize(this, prefab.name);
        return pooledGO;
    }

    //All GameObjects which are in the pool are disabled, so when we get an instance of the GameObject we need to enable it.
    private void OnGetFromPool(PooledGameObject pooledGO)
    {
        pooledGO.SetActive(true);
    }


    //Returning the GameObject back to the pool.
    private void OnReleaseToPool(PooledGameObject releaseGO)
    {
        releaseGO.transform.SetParent(transform);
        releaseGO.SetActive(false);
    }

    //When pool gets destroyed we want to clean up the allocated GameObjects as well.
    private void OnDestroyPoolObject(PooledGameObject destroyedGO)
    {
        Destroy(destroyedGO.gameObject);
    }

    public PooledGameObject Spawn(string id, Transform parent = null)
    {
        if (pool.TryGetValue(id, out var objectPool))
        {
            var pooledGO = objectPool.Get();
            if (parent)
            {
                pooledGO.transform.SetParent(parent);
            }

            return pooledGO;
        }

        return null;
    }

    public void AddToPool(PoolItem item)
    {
        foreach (var obj in poolableObjects)
        {
            if (item.Prefab.name == obj.Prefab.name)
            {
                obj.PoolCount = item.PoolCount;
                return;
            }
        }

        poolableObjects.Add(item);
    }

    public void RemoveFromPool(PoolItem item)
    {
        foreach (var obj in poolableObjects)
        {
            if (item.Prefab.name == obj.Prefab.name)
            {
                poolableObjects.Remove(obj);
            }
        }

    }
}
