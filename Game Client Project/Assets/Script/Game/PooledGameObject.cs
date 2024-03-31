using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PooledGameObject : MonoBehaviour
{
    private IObjectPool<PooledGameObject> pool;
    private PoolManager poolManager;

    public string Id { get; private set; }
    public bool IsRelease { get; private set; }

    public void Initialize(PoolManager poolMgr, string id)
    {
        pool = poolMgr.Pool[id];
        poolManager = poolMgr;
        Id = id;
        poolManager.OnCleanup += OnCleanup;
        poolManager.OnRelease += OnRelease;
    }

    public void SetActive(bool active)
    {
        // Sets gameObject ready to used
        gameObject.SetActive(active);
        IsRelease = !active;
    }

    public void OnRelease()
    {
        if (IsRelease)
        {
            return;
        }

        // Return object to pool
        pool.Release(this);
    }

    private void OnDestroy()
    {
        // gameObject gets destroyed unregister from pool
        if (poolManager)
        {
            poolManager.OnCleanup -= OnCleanup;
            poolManager.OnRelease -= OnRelease;
        }
    }

    private void OnCleanup()
    {
        // Called when pool manager gets cleanup
        OnRelease();
        Destroy(gameObject);
    }
}
