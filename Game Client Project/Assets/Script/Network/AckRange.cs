using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AckRange
{
    uint mStart;
    uint mCount;

    public AckRange()
    {
        mStart = 0;
        mCount = 0;
    }

    public AckRange(uint mStart)
    {
        this.mStart = mStart;
        this.mCount = 1;
    }

    public bool ExtendIfShould(uint inSequenceNumber)
    {
        if (inSequenceNumber == mStart + mCount)
        {
            ++mCount;
            return true;
        }
        else
        {
            return false;
        }
    }

    public uint GetStart() { return mStart; }
    public uint GetCount() { return mCount; }

    public void Write(OutputMemoryBitStream inOutputStream)
    {
        inOutputStream.Write(mStart);
        bool hasCount = mCount > 1;
        inOutputStream.Write(hasCount);
        if (hasCount)
        {
            //most you can ack is 255...
            uint countMinusOne = mCount - 1;
            byte countToAck = (byte)(countMinusOne > 255 ? 255 : countMinusOne);
            inOutputStream.Write(countToAck);
        }
    }

    public void Read(InputMemoryBitStream inInputStream)
    {
        inInputStream.Read(out mStart);
        bool hasCount;
        inInputStream.Read(out hasCount);
        if (hasCount)
        {
            byte countMinusOne;
            inInputStream.Read(out countMinusOne);
            mCount = (uint)countMinusOne + 1;
        }
        else
        {
            //default!
            mCount = 1;
        }
    }

}
