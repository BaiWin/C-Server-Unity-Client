using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ReceivedPacket
{
    float mReceivedTime;
    InputMemoryBitStream mPacketBuffer;
    SocketAddress mFromAddress;

    public ReceivedPacket(float inReceivedTime, InputMemoryBitStream inInputMemoryBitStream, SocketAddress inAddress)
    {
        this.mReceivedTime = inReceivedTime;
        this.mPacketBuffer = inInputMemoryBitStream;
        this.mFromAddress = inAddress;
    }
     
    public SocketAddress GetFromAddress() { return mFromAddress; }
    public float GetReceivedTime() { return mReceivedTime; }
    public InputMemoryBitStream GetPacketBuffer() { return mPacketBuffer; }

}
