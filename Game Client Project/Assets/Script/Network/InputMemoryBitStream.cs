using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InputMemoryBitStream : MemoryBitStream
{
    byte[] mBuffer;
    uint mBitHead;
    uint mBitCapacity;
    bool mIsBufferOwner;

    public InputMemoryBitStream(byte[] inBuffer, uint inBitCount)
    {
        mBuffer = inBuffer;
        mBitCapacity = inBitCount;
        mBitHead = 0;
        mIsBufferOwner = false;
    }

    public InputMemoryBitStream(InputMemoryBitStream inOther)
    {
        mBitCapacity = inOther.mBitCapacity;
        mBitHead = inOther.mBitHead;
        mIsBufferOwner = true;

        int byteCount = (int)mBitCapacity / 8;
        mBuffer = new byte[byteCount];
    }
    ~InputMemoryBitStream() { if (mIsBufferOwner) { mBuffer = null; }; }

    public uint GetRemainingBitCount() { return mBitCapacity - mBitHead; }


    // Read from single byte
    unsafe void ReadBits(ref byte outData, uint inBitCount)
    {
        uint byteOffset = mBitHead >> 3;
        uint bitOffset = mBitHead & 0x7;

        outData = (byte)(mBuffer[byteOffset] >> (byte)bitOffset);

        uint bitsFreeThisByte = 8 - bitOffset;
        if (bitsFreeThisByte < inBitCount)
        {
            outData |= (byte)(mBuffer[byteOffset + 1] << (byte)bitsFreeThisByte);
        }

        outData &= (byte)(~(0x00ff << (int)inBitCount));

        mBitHead += inBitCount;
    }

    // Read from byte array
    unsafe void ReadBits(ref void* outData, uint inBitCount)
    {
        byte* destByte = (byte*)outData;
        int offset = 0;
        while (inBitCount > 8)
        {
            ReadBits(ref destByte[offset], 8);
            offset++;
            inBitCount -= 8;
        }
        if(inBitCount > 0)
        {
            ReadBits(ref destByte[offset], inBitCount);
        }
        outData = destByte;
    }

    //public unsafe void Read<T>(T outData) where T : UnityEngine.Object
    //{
    //    //uint inBitCount = (uint)Helper.SizeOf(typeof(T));
    //    //T readValue = default;
    //    //void* ptr = &readValue;
    //    ////byte[] outBytes = Helper.ToFlipped<T>(outData);
    //    //ReadBits(ref ptr, inBitCount);
    //}

    public unsafe void Read(out uint outData, uint inBitCount = 32) 
    {
        uint readValue = 0;
        void* ptr = (void*)&readValue;
        ReadBits(ref ptr, inBitCount);
        outData = *(uint*)ptr;
    }
    public unsafe void Read(out int outData, uint inBitCount = 32)
    {
        int readvalue = 0;
        void* ptr = (void*)&readvalue;
        ReadBits(ref ptr, inBitCount);
        outData = *(int*)ptr;
    }
    public unsafe void Read(out float outData) 
    {
        float readValue = 0;
        void* ptr = (void*)&readValue;
        ReadBits(ref ptr, 32);
        outData = *(float*)ptr;
    }
    public unsafe void Read(out bool outData) 
    {
        byte readValue = 0;
        void* ptr = (void*)&readValue;
        ReadBits(ref ptr, 1);
        outData = Convert.ToBoolean(*(byte*)ptr);
    }
    public unsafe void Read(out byte outData, uint inBitCount = 8)
    {
        byte readValue = 0;
        void* ptr = (void*)&readValue;
        ReadBits(ref ptr, inBitCount);
        outData = *(byte*)ptr;
    }
    public void Read(out Vector3 outVector)
    {
        Read(out outVector.x);
        Read(out outVector.y);
        Read(out outVector.z);
    }
    public void Read(out Quaternion outQuat)
    {
        float precision = (2.0f / 65535.0f);

        uint f = 0;

        Read(out f, 16);
        outQuat.x = ConvertFromFixed(f, -1.0f, precision);
        Read(out f, 16);
        outQuat.y = ConvertFromFixed(f, -1.0f, precision);
        Read(out f, 16);
        outQuat.z = ConvertFromFixed(f, -1.0f, precision);

        outQuat.w = Mathf.Sqrt(1.0f -
                                outQuat.x * outQuat.x +
                                outQuat.y * outQuat.y +
                                outQuat.z * outQuat.z);

        bool isNegative = false;
        Read(out isNegative);

        if (isNegative)
        {
            outQuat.x *= -1;
        }
    }
    public unsafe void Read(out string inString)
    {
        uint elementCount;
        Read(out elementCount);
        fixed (char* stringPtr = inString)
        {
            void* ptr = stringPtr;
            ReadBits(ref ptr, elementCount * 8);
            char* ptrChar = (char*)ptr;
            inString = Marshal.PtrToStringAnsi((IntPtr)ptrChar);
        }
    }

    public void ResetToCapacity(uint inByteCapacity) { mBitCapacity = inByteCapacity << 3; mBitHead = 0; }

    
}
