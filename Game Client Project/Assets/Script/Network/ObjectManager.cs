using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectManager : MonoSingleton<ObjectManager>
{
    private Dictionary<int, IProduct> networkObjectsMap = new Dictionary<int, IProduct>();

    public void AddToNetworkIdToGameObjectMap(int networkId, IProduct inGameObject)
    {
        networkObjectsMap.Add(networkId, inGameObject);
    }

    public void RemoveFromNetworkIdToGameObjectMap(int networkId)
    {
        if (networkObjectsMap.ContainsKey(networkId))
        {
            networkObjectsMap.Remove(networkId);
        }
        else
        {
            Debug.Log("The object you are trying to remove already not exsited, Is it remove somewhere else ?");
        }
    }

    public IProduct GetGameObject(int inNetworkId )
    {
        if(networkObjectsMap.ContainsKey(inNetworkId))
        {
            return networkObjectsMap[inNetworkId];
        }
        else
        {
            return null;
        }
    }

    public Dictionary<int, IProduct> GetNetWorkObjectsMap()
    {
        return networkObjectsMap;
    }

    public void ResetAndClear()
    {
        networkObjectsMap.Clear();
    }

    //void Update()
    //{
    //    foreach (var go in networkObjectsMap.Keys.ToList<int>())
    //    {
    //        if (((ProductCommon)networkObjectsMap[go]).DoesWantToDie())
    //        {
    //            networkObjectsMap.Remove(go);
    //            ((ProductCommon)networkObjectsMap[go]).pooledSelf.OnRelease();
    //        }
    //    }
    //}
}
