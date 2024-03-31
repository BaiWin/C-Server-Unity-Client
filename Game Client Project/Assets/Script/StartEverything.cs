using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartEverything : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject bugPrefab;
    public GameObject projectilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        PoolItem player = new PoolItem(playerPrefab);
        PoolManager.Instance.AddToPool(player);
        PoolItem bug = new PoolItem(bugPrefab);
        PoolManager.Instance.AddToPool(bug);
        PoolItem projectile = new PoolItem(projectilePrefab);
        PoolManager.Instance.AddToPool(projectile);
        PoolManager.Instance.InstantiatePool();
        


        //PoolManager.Instance.Spawn("Bug");
        //PoolManager.Instance.Spawn("Bug");

        if (NetworkClient.Instance) { };
        if (InputManager.Instance) { };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
