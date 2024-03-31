using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using UnityEngine;

public enum NetworkClientState
{
    NCS_Uninitialized,
    NCS_SayingHello,
    NCS_Welcomed
};

public class NetworkClient : Network
{
    const float kTimeBetweenHellos = 1.0f;
    const float kTimeBetweenInputPackets = 0.033f;

    DeliveryNotificationManager mDeliveryNotificationManager;
    ReplicationManagerClient mReplicationManagerClient = new ReplicationManagerClient();

    NetworkClientState mState;

    float mTimeOfLastHello;
    float mTimeOfLastInputPacket;

    string mName;
    int mPlayerId;

    float mLastPacketFromServerTime;

    float mLastMoveProcessedByServerTimestamp;

    WeightedTimedMovingAverage mAvgRoundTripTime;
    float mLastRoundTripTime;

    [HideInInspector]
    public bool isInitializedAndConnected;

    public override void Start()
    {
        base.Start();
        mState = NetworkClientState.NCS_Uninitialized;

        mAvgRoundTripTime = new WeightedTimedMovingAverage(1.0f);
        //Init("0");
    }

    private void Update()
    {
        if (!isInitializedAndConnected) return;

        ProcessIncomingPackets();
        CheckForDisconnects();
        SendOutgoingPackets();
    }

    public void Init(string inName, string IPAddress, int port )
    {
	    base.Init(IPAddress, port);   // 11000

	    //mServerAddress = inServerAddress;
	    mState = NetworkClientState.NCS_SayingHello;
	    mTimeOfLastHello = 0;
	    mName = inName;

        isInitializedAndConnected = true;

        // Set the last pack time from server, so check disconnect wont be true when connecting
        mLastPacketFromServerTime = Time.time;

        // All moved from start to init
        mDeliveryNotificationManager = new DeliveryNotificationManager(true, false);
        mLastRoundTripTime = 0.0f;
        
    }

    float mClientDisconnectTimeout = 4f;
    public void CheckForDisconnects()
    {
        if(Time.time - mClientDisconnectTimeout > mLastPacketFromServerTime)
        {
            this.ResetAndClear();
            ObjectManager.Instance.ResetAndClear();
            InputManager.Instance.ResetAndClear();
            FindObjectOfType<UIController>().LoginPanelVisible(true);
            PoolManager.Instance.ReleasePooledObject();
        }
    }

    public override void ProcessPacket(InputMemoryBitStream inInputStream)
    {
        mLastPacketFromServerTime = Time.time;

	    uint packetType;
        inInputStream.Read(out packetType);
        switch (Helper.GetCodeFromInt(packetType))
	    {
	        case kWelcomeCC:
		        HandleWelcomePacket(inInputStream );
		        break;
	        case kStateCC:
                #region Chapter8
                if (mDeliveryNotificationManager.ReadAndProcessState(inInputStream))
                {
                #endregion
                    HandleStatePacket(inInputStream);
                #region Chapter8
                }
                #endregion
                break;
	    }
    }

    public WeightedTimedMovingAverage	GetAvgRoundTripTime() { return mAvgRoundTripTime; }
    public float GetRoundTripTime() { return mAvgRoundTripTime.GetValue(); }
    public float GetLastMoveProcessedByServerTimestamp() { return mLastMoveProcessedByServerTimestamp; }

    void SendOutgoingPackets()
    {
        switch (mState)
        {
            case NetworkClientState.NCS_SayingHello:
                UpdateSayingHello();
                break;
            case NetworkClientState.NCS_Welcomed:
                UpdateSendingInputPacket();
                break;
        }
    }

    void UpdateSayingHello()
    {
        float time = Timing.Instance.GetTime();

        if (time > mTimeOfLastHello + kTimeBetweenHellos)
        {
            SendHelloPacket();
            mTimeOfLastHello = time;
        }
    }

    void SendHelloPacket()
    {
        OutputMemoryBitStream helloPacket = new OutputMemoryBitStream();

        helloPacket.Write(Helper.GetCodeFromString(kHelloCC));
        helloPacket.Write(mName);

        SendPacket(helloPacket);
    }

    void HandleWelcomePacket(InputMemoryBitStream inInputStream )
    {
        if (mState == NetworkClientState.NCS_SayingHello)
        {
            //if we got a player id, we've been welcomed!
            int playerId;
            inInputStream.Read(out playerId);
            mPlayerId = playerId;
            mState = NetworkClientState.NCS_Welcomed;
            Debug.Log(string.Format("{0} was welcomed on client as player {1}", mName, mPlayerId));
        }
    }

    void HandleStatePacket(InputMemoryBitStream inInputStream)
    {
        if (mState == NetworkClientState.NCS_Welcomed)
        {
            ReadLastMoveProcessedOnServerTimestamp(inInputStream);

            //old
            //HandleGameObjectState( inPacketBuffer );
            HandleScoreBoardState(inInputStream);

            //tell the replication manager to handle the rest...
            mReplicationManagerClient.Read(inInputStream);
        }
    }

    void ReadLastMoveProcessedOnServerTimestamp(InputMemoryBitStream inInputStream )
    {
        bool isTimestampDirty = false;
        inInputStream.Read(out isTimestampDirty);
        if (isTimestampDirty)
        {
            inInputStream.Read(out mLastMoveProcessedByServerTimestamp);

            float rtt = Time.time - mLastMoveProcessedByServerTimestamp;
            mLastRoundTripTime = rtt;
            mAvgRoundTripTime.Update(rtt);

            InputManager.Instance.GetMoveList().RemovedProcessedMoves(mLastMoveProcessedByServerTimestamp);

        }
    }

    void HandleScoreBoardState(InputMemoryBitStream inInputStream )
    {
        ScoreBoardManager.Instance.Read(inInputStream);
    }

    //deprecated
    void HandleGameObjectState(InputMemoryBitStream inInputStream)
    {
        List<int> objectsUpdated = new List<int>();
        int stateCount;
        inInputStream.Read(out stateCount);
        if (stateCount > 0)
        {
            foreach (ProductCommon item in ObjectManager.Instance.GetNetWorkObjectsMap().Values)
            {
                item.SetDoesWantToDie(true);
            }

            for (int stateIndex = 0; stateIndex < stateCount; ++stateIndex)
            {
                int networkId;
                uint fourCC;

                inInputStream.Read(out networkId);
                inInputStream.Read(out fourCC);
                
                ProductCommon go = (ProductCommon)ObjectManager.Instance.GetGameObject(networkId);
                //didn't find it, better create it!
                if (go == null)
                {
                    go = (ProductCommon)ConcreteBugFactory.Instance.GetProduct(Helper.GetCodeFromInt(fourCC), Vector3.zero);
                    go.SetNetworkId(networkId);
                    ObjectManager.Instance.AddToNetworkIdToGameObjectMap(networkId, go);
                }

                //now we can update into it
                go.Read(inInputStream);
                go.SetDoesWantToDie(false);
            }
        }

        //anything left gets the axe
        foreach (var item in ObjectManager.Instance.GetNetWorkObjectsMap())
        {
            if(((ProductCommon)item.Value).DoesWantToDie())
            {
                ObjectManager.Instance.RemoveFromNetworkIdToGameObjectMap(item.Key);
            }
        }
    }

    void UpdateSendingInputPacket()
    {
        float time = Time.time;

        if (time > mTimeOfLastInputPacket + kTimeBetweenInputPackets)
        {
            SendInputPacket();
            mTimeOfLastInputPacket = time;
        }
    }

    void SendInputPacket()
    {
        MoveList moveList = InputManager.Instance.GetMoveList();

        if (moveList.HasMoves())
        {
            OutputMemoryBitStream inputPacket = new OutputMemoryBitStream();
            inputPacket.Write(Helper.GetCodeFromString(kInputCC));

            mDeliveryNotificationManager.WriteState(inputPacket);

            //we only want to send the last three moves
            int moveCount = moveList.GetMoveCount();
            int startIndex = moveCount > 3 ? moveCount - 3 - 1 : 0;
            inputPacket.Write(moveCount - startIndex, 2);
            for (int i = startIndex; i < moveCount; ++i)
            {
                moveList.GetAtIndex(i).Write(inputPacket);
            }

            SendPacket(inputPacket);
            #region Chapter8
            //moveList.Clear();
            
            moveList.Clear();  //mod
            #endregion
        }
    }

    public int GetPlayerId() { return mPlayerId; }

    public override void ResetAndClear()
    {
        base.ResetAndClear();
        mDeliveryNotificationManager = null;
        mState = NetworkClientState.NCS_Uninitialized;
        mTimeOfLastHello = 0;
        mTimeOfLastInputPacket = 0;
        mName = "";
        mPlayerId = -1;
        mLastPacketFromServerTime = 0;
        mLastMoveProcessedByServerTimestamp = 0;
        mLastRoundTripTime = 0;
        isInitializedAndConnected = false;

        
    }
}
