using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InFlightPacket
{
    uint mSequenceNumber;
    float mTimeDispatched;

    Dictionary<int, TransmissionData> mTransmissionDataMap = new Dictionary<int, TransmissionData>();

    public InFlightPacket(uint mSequenceNumber)
    {
        this.mSequenceNumber = mSequenceNumber;
        mTimeDispatched = Time.time;
    }

    public uint GetSequenceNumber(){ return mSequenceNumber; }
    public float GetTimeDispatched() { return mTimeDispatched; }

    // This is server only
    //public void SetTransmissionData(int inKey, TransmissionData inTransmissionData)
    //{
    //    mTransmissionDataMap[inKey] = inTransmissionData;
    //}
 //   public TransmissionData GetTransmissionData( int inKey )
	//{
	//	if( mTransmissionDataMap.ContainsKey(inKey))
 //       {
 //           return mTransmissionDataMap[inKey];
 //       }
 //       else { return null; }
	//}
	
	public void HandleDeliveryFailure(DeliveryNotificationManager inDeliveryNotificationManager)
    {
        foreach (var pair in mTransmissionDataMap)
        {
            pair.Value.HandleDeliveryFailure(inDeliveryNotificationManager);
        }
    }
    public void HandleDeliverySuccess(DeliveryNotificationManager inDeliveryNotificationManager)
    {
        foreach (var pair in mTransmissionDataMap)
        {
            pair.Value.HandleDeliverySuccess(inDeliveryNotificationManager);
        }
    }
}
