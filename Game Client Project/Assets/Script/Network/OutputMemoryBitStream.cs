using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using UnityEngine;

public class OutputMemoryBitStream : MemoryBitStream
{
    byte[] mBuffer;
    uint mBitHead;
    uint mBitCapacity;
    bool mIsBufferOwner;

    public OutputMemoryBitStream()
    {
        mBitHead = 0;
        mBuffer = null;
        ReallocBuffer(1024 * 8);
    }

    ~OutputMemoryBitStream() { if (mIsBufferOwner) { mBuffer = null; }; }

    UInt32 GetRemainingBitCount() { return mBitCapacity - mBitHead; }

    void WriteBits(ref byte inData, uint inBitCount)
    {
        uint nextBitHead = mBitHead + inBitCount;

        if(nextBitHead > mBitCapacity)
        {
            ReallocBuffer((uint)Mathf.Max(mBitCapacity * 2, nextBitHead));
        }

        uint byteOffset = mBitHead >> 3;
        uint bitOffset = mBitHead & 0x7;

        byte currentMask = (byte)~(0xff << (int)bitOffset);
        mBuffer[byteOffset] = (byte)((mBuffer[byteOffset] & currentMask) | (inData << (byte)bitOffset));

        uint bitsFreeThisByte = 8 - bitOffset;
        if (bitsFreeThisByte < inBitCount)
        {
            mBuffer[byteOffset + 1] = (byte)(inData >> (byte)bitsFreeThisByte);
        }

        mBitHead = nextBitHead;
    }

    public void WriteBits(byte[] inData, uint inBitCount)
    {
        int i = 0;
        while( inBitCount > 8)
        {
            WriteBits(ref inData[i], 8);
            i++;
            inBitCount -= 8;
        }
        if (inBitCount > 0)
        {
            WriteBits(ref inData[i], inBitCount);
        }
    }

    public void Write<T>(T outData) where T: UnityEngine.Object
    {
        uint inBitCount = (uint)Helper.SizeOf(typeof(T));
        byte[] outBytes = Helper.ToFlipped<T>(outData);
        WriteBits(outBytes, inBitCount);
    }
    public void Write(uint outData, uint inBitCount = 32) { WriteBits(BitConverter.GetBytes(outData), inBitCount); }
    public void Write(int outData, uint inBitCount = 32) { WriteBits(BitConverter.GetBytes(outData), inBitCount); }
    public void Write(float outData, uint inBitCount = 32) { WriteBits(BitConverter.GetBytes(outData), inBitCount); }
    public void Write(byte outData) { WriteBits(BitConverter.GetBytes(outData), 8); }
    public void Write(bool outData) { WriteBits(BitConverter.GetBytes(outData), 1); }
    public void Write(Vector3 outVector)
    {
        Write(outVector.x);
        Write(outVector.y);
        Write(outVector.z);
    }
    public void Write(Quaternion outQuat)
    {
        float precision = (2.0f / 65535.0f);

        uint f = 0;

        Write(f, 16);
        outQuat.x = ConvertFromFixed(f, -1.0f, precision);
        Write(f, 16);
        outQuat.y = ConvertFromFixed(f, -1.0f, precision);
        Write(f, 16);
        outQuat.z = ConvertFromFixed(f, -1.0f, precision);

        outQuat.w = Mathf.Sqrt(1.0f -
                                outQuat.x * outQuat.x +
                                outQuat.y * outQuat.y +
                                outQuat.z * outQuat.z);

        bool isNegative = false;
        Write(isNegative);

        if (isNegative)
        {
            outQuat.x *= -1;
        }
    }

    public void Write(string inString)
    {
        uint elementCount = (uint)inString.Length;
        Write(elementCount);
        char[] chars = inString.ToCharArray();
        foreach (var i in chars)
        {
            Write((byte)i);
        }
    }


    public void ReallocBuffer(uint inNewBitLength)
    {
        if (mBuffer == null)
        {
            mBuffer = new byte[inNewBitLength >> 3];
        }
        else
        {
            byte[] tempBuffer = new byte[inNewBitLength >> 3];
            Array.Copy(mBuffer, tempBuffer, mBuffer.Length);
            mBuffer = tempBuffer;
        }

        //handle realloc failure
        //...
        mBitCapacity = inNewBitLength;
    }

    public byte[] GetBuffer() { return mBuffer; }
    public int GetBitLength() { return (int)mBitHead; }
    public int GetByteLength() { return ((int)mBitHead + 7) >> 3; }
}
