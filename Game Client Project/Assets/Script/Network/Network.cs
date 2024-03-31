using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;



public class Network : MonoSingleton<Network>
{
    #region CCState
    public const string kHelloCC = "HELO";
    public const string kWelcomeCC = "WLCM";
    public const string kStateCC = "STAT";
    public const string kInputCC = "INPT";
    #endregion

    public const int kMaxPacketsPerFrameCount = 10;

    Queue<ReceivedPacket> mPacketQueue = new Queue<ReceivedPacket>();

    MyUDPClient mSocket;

    WeightedTimedMovingAverage mBytesReceivedPerSecond;
    WeightedTimedMovingAverage mBytesSentPerSecond;

    float lastTimeReceive = 0;
    int mBytesSentThisFrame;

    [SerializeField]
    private float mDropPacketChance;
    [SerializeField]
    private float mSimulatedLatency;

    private byte[] receiveBuffer;
    private byte[] sendBuffer;
    //private int receiveByteLength;

    public Network()
    {
        this.mBytesSentThisFrame = 0;
        this.mDropPacketChance = 0;
        this.mSimulatedLatency = 0;
    }

    public virtual void Start()
    {
        receiveBuffer = new byte[1024];
        sendBuffer = new byte[1024];

        mBytesReceivedPerSecond = new WeightedTimedMovingAverage(1.0f);
        mBytesSentPerSecond = new WeightedTimedMovingAverage(1.0f);
    }

    public void Init(string ipAddress, int inPort)
    {
        //mSocket = new MyUDPClient(IPAddress.Any.ToString(), inPort, receiveBuffer, sendBuffer);
        mSocket = new MyUDPClient(ipAddress, inPort, receiveBuffer, sendBuffer);   // "192.168.16.122"
        

        mSocket.Connect();
        mSocket.Receive();
        mSocket.OnMsgParse -= ReadIncomingPacketsIntoQueue;
        mSocket.OnMsgParse += ReadIncomingPacketsIntoQueue;

    }

    public int GetCCStateCode(string inCCState)
    {
        byte[] CC = Encoding.ASCII.GetBytes(inCCState);
        Array.Reverse(CC);
        return BitConverter.ToInt32(CC, 0);
    }

    public void ProcessIncomingPackets()
    {
        //ReadIncomingPacketsIntoQueue();

        ProcessQueuedPackets();

        UpdateBytesSentLastFrame();

    }

    public int totalReadByteCount = 0;
    void ReadIncomingPacketsIntoQueue(byte[] receiveBytes)
    {
        InputMemoryBitStream inputStream = new InputMemoryBitStream(receiveBytes, (uint)receiveBytes.Length * 8 );
        inputStream.ResetToCapacity((uint)receiveBytes.Length);

        var rand = new System.Random();
        if (rand.Next() >= mDropPacketChance)
        {
            //we made it
            //shove the packet into the queue and we'll handle it as soon as we should...
            //we'll pretend it wasn't received until simulated latency from now
            //this doesn't sim jitter, for that we would need to.....

            float simulatedReceivedTime = Timing.Instance.GetTime() + mSimulatedLatency;
            mPacketQueue.Enqueue(new ReceivedPacket(simulatedReceivedTime, inputStream, null));
        }
        else
        {
            Debug.Log("Dropped packet!");
            //dropped!
        }

        if (Timing.Instance.GetTime() - lastTimeReceive < 1)
        {
            totalReadByteCount += receiveBytes.Length;
        }
        else
        {
            mBytesReceivedPerSecond.UpdatePerSecond(totalReadByteCount);
            lastTimeReceive = Timing.Instance.GetTime();
            totalReadByteCount = 0;
        }
        // Take care of something can't be called in background thread
    }

    void ProcessQueuedPackets()
    {
        //look at the front packet...
        while (mPacketQueue.Count != 0)
        {
            ReceivedPacket nextPacket = mPacketQueue.Peek();
            if (Time.time > nextPacket.GetReceivedTime())
            {
                ProcessPacket(nextPacket.GetPacketBuffer());
                mPacketQueue.Dequeue();
            }
            else
            {
                break;
            }

        }

    }

    public virtual void ProcessPacket(InputMemoryBitStream inInputStream) { }

    public WeightedTimedMovingAverage GetBytesReceivedPerSecond() { return mBytesReceivedPerSecond; }
    public WeightedTimedMovingAverage GetBytesSentPerSecond() { return mBytesSentPerSecond; }
    public float GetSimulatedLatency() { return mSimulatedLatency; }
    public float GetDropPacketChance() { return mDropPacketChance; }
    public void SetDropPacketChance(float inChance) { mDropPacketChance = inChance; }
    public void SetSimulatedLatency(float inLatency) { mSimulatedLatency = inLatency; }

    public void SendPacket(OutputMemoryBitStream inOutputStream)
    {
        int sentByteCount = mSocket.Send(inOutputStream.GetBuffer(), inOutputStream.GetByteLength());
        if (sentByteCount > 0)
        {
            mBytesSentThisFrame += sentByteCount;
        }
    }

    void UpdateBytesSentLastFrame()
    {
        if (mBytesSentThisFrame > 0)
        {
            mBytesSentPerSecond.UpdatePerSecond(mBytesSentThisFrame);

            mBytesSentThisFrame = 0;
        }
    }

    public void CloseUDP()
    {
        mSocket.Close();
    }

    public virtual void ResetAndClear()
    {
        mSocket.Close();
        mSocket = null;
        lastTimeReceive = 0;
        mBytesSentThisFrame = 0;
        mDropPacketChance = 0;
        mSimulatedLatency = 0;
    }
}
