using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ReplicationManagerTransmissionData : TransmissionData
{
    List<ReplicationTransmission> mTransmissions = new List<ReplicationTransmission>();
    class ReplicationTransmission
    {
        int mNetworkId;
        ReplicationAction mAction;
        uint mState;

        public int GetNetworkId() { return mNetworkId; }
        public ReplicationAction GetAction() { return mAction; }
        public uint GetState() { return mState; }

        public ReplicationTransmission(int mNetworkId, ReplicationAction mAction, uint mState)
        {
            this.mNetworkId = mNetworkId;
            this.mAction = mAction;
            this.mState = mState;
        }
    }
    public void AddTransmission(int inNetworkId, ReplicationAction inAction, uint inState)
    {
        /*
	    //it would be silly if we already had a transmission for this network id in here...
	    for( const auto& transmission: mTransmissions )
	    {   
	    	assert( inNetworkId != transmission.GetNetworkId() );
	    }
	    */
        mTransmissions.Add(new ReplicationTransmission(inNetworkId, inAction, inState));
    }

    public override void HandleDeliveryFailure(DeliveryNotificationManager inDeliveryNotificationManager)
    {
        //run through the transmissions
        foreach (var transmission in mTransmissions )
	    {
            //is it a create? then we have to redo the create.
            int networkId = transmission.GetNetworkId();

            switch (transmission.GetAction())
            {
                case ReplicationAction.RA_Create:
                    HandleCreateDeliveryFailure(networkId);
                    break;
                case ReplicationAction.RA_Update:
                    HandleUpdateStateDeliveryFailure(networkId, transmission.GetState(), inDeliveryNotificationManager);
                    break;
                case ReplicationAction.RA_Destroy:
                    HandleDestroyDeliveryFailure(networkId);
                    break;
            }

        }
    }

    public override void HandleDeliverySuccess(DeliveryNotificationManager inDeliveryNotificationManager)
    {
        //run through the transmissions, if any are Destroyed then we can remove this network id from the map
        foreach (var transmission in mTransmissions)
        {
            switch (transmission.GetAction())
            {
                case ReplicationAction.RA_Create:
                    HandleCreateDeliverySuccess(transmission.GetNetworkId());
                    break;
                case ReplicationAction.RA_Destroy:
                    HandleDestroyDeliverySuccess(transmission.GetNetworkId());
                    break;
            }
        }
    }

    void HandleCreateDeliveryFailure(int inNetworkId)
    {
        throw new Exception("Creation should always be raised by server");
    }

    void HandleUpdateStateDeliveryFailure(int inNetworkId, uint inState, DeliveryNotificationManager inDeliveryNotificationManager)
    {
        throw new Exception("Update should always be raised by server");
    }

    void HandleDestroyDeliveryFailure(int inNetworkId)
    {
        throw new Exception("Destroy should always be raised by server");
    }

    void HandleCreateDeliverySuccess(int inNetworkId)
    {
        throw new Exception("Creation should always be raised by server");
    }

    void HandleDestroyDeliverySuccess(int inNetworkId)
    {
        throw new Exception("Destroy should always be raised by server");
    }

}
