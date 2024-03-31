using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMemoryStream
{
    void Read(InputMemoryBitStream inInputStream);

    void Write(OutputMemoryBitStream inOutputStream);
}
