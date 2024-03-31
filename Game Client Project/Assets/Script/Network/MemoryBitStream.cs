using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryBitStream
{
    protected uint ConvertToFixed(float inNumber, float inMin, float inPrecision)
    {
        return (uint)((inNumber - inMin) / inPrecision);
    }

    protected float ConvertFromFixed(uint inNumber, float inMin, float inPrecision)
    {
        return (float)(inNumber) * inPrecision + inMin;
    }
}
