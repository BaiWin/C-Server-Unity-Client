using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkObject
{
    int mNetworkId { get; set; }

    void Read(InputMemoryBitStream inInputStream);
    uint Write(OutputMemoryBitStream inOutputStream, uint inDirtyState);
}
