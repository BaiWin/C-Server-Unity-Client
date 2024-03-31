using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class ReplicationManagerClient
{
    public void Read(InputMemoryBitStream inInputStream)
    {
        while (inInputStream.GetRemainingBitCount() >= 32)
        {
            //read the network id...
            int networkId = -1; 
            inInputStream.Read(out networkId);

            //only need 2 bits for action...
            int action = -1; inInputStream.Read(out action, 2);

            switch (action)
            {
                case (int)ReplicationAction.RA_Create:
                    ReadAndDoCreateAction(inInputStream, networkId);
                    break;
                case (int)ReplicationAction.RA_Update:
                    ReadAndDoUpdateAction(inInputStream, networkId);
                    break;
                case (int)ReplicationAction.RA_Destroy:
                    ReadAndDoDestroyAction(inInputStream, networkId);
                    break;
            }
        }
    }

    void ReadAndDoCreateAction(InputMemoryBitStream inInputStream, int inNetworkId)
    {
        //need 4 cc
        uint fourCCName;
        inInputStream.Read(out fourCCName);

        //we might already have this object- could happen if our ack of the create got dropped so server resends create request 
        //( even though we might have created )
        ProductCommon gameObject = (ProductCommon)ObjectManager.Instance.GetGameObject(inNetworkId);
        if (!gameObject)
        {
            //create the object and map it...
            gameObject = (ProductCommon)ConcreteBugFactory.Instance.GetProduct(Helper.GetCodeFromInt(fourCCName), Vector3.zero);
           
            gameObject.SetNetworkId(inNetworkId);
            ObjectManager.Instance.AddToNetworkIdToGameObjectMap(inNetworkId, gameObject);

            //Object will not be created if not registed in pool
        }

        //and read state
        gameObject.Read(inInputStream);
    }

    void ReadAndDoUpdateAction(InputMemoryBitStream inInputStream, int inNetworkId)
    {
        //need object
        ProductCommon gameObject = (ProductCommon)ObjectManager.Instance.GetGameObject(inNetworkId);

        //gameObject MUST be found, because create was ack'd if we're getting an update...
        //and read state
        gameObject.Read(inInputStream);
    }

    void ReadAndDoDestroyAction(InputMemoryBitStream inInputStream, int inNetworkId)
    {
        //if something was destroyed before the create went through, we'll never get it
        //but we might get the destroy request, so be tolerant of being asked to destroy something that wasn't created
        ProductCommon gameObject = (ProductCommon)ObjectManager.Instance.GetGameObject(inNetworkId);
        if (gameObject)
        {
            gameObject.SetDoesWantToDie(true);
            ObjectManager.Instance.RemoveFromNetworkIdToGameObjectMap(inNetworkId);
            gameObject.pooledSelf.OnRelease();
            // Maybe this code should be moved to somewhere else for optimization?
        }
    }
}
