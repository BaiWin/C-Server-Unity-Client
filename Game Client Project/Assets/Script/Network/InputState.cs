using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputState : IMemoryStream
{
    float horizontal;
    float vertical;
    float yRotation;
    bool isShooting;
    bool isSwitchColor;
    byte mColor;

    public InputState()
    {
        this.horizontal = 0;
        this.vertical = 0;
        this.yRotation = 0;
        this.isShooting = false;
        this.isSwitchColor = false;
        this.mColor = 0;
    }

    public InputState(InputState state)
    {
        this.horizontal = state.horizontal;
        this.vertical = state.vertical;
        this.yRotation = state.yRotation;
        this.isShooting = state.isShooting;
        this.isSwitchColor = state.isSwitchColor;
        this.mColor = state.mColor;
    }

    public void Read(InputMemoryBitStream inInputStream)
    {
        inInputStream.Read(out horizontal);
        inInputStream.Read(out vertical);
        inInputStream.Read(out yRotation);
        inInputStream.Read(out isShooting);
        inInputStream.Read(out isSwitchColor);
        if (isSwitchColor)
        {
            inInputStream.Read(out mColor);
        }
        
    }

    public void Write(OutputMemoryBitStream inOutputStream)
    {
        inOutputStream.Write(horizontal);
        inOutputStream.Write(vertical);
        inOutputStream.Write(yRotation);
        inOutputStream.Write(isShooting);
        inOutputStream.Write(isSwitchColor);
        if (isSwitchColor)
        {
            inOutputStream.Write(mColor);
        }
    }

    public void UpdateInputValue(float horizontal, float vertical, float yRotation, bool isShooting, bool isSwitchColor, byte mColor)
    {
        this.horizontal = horizontal;
        this.vertical = vertical;
        this.yRotation = yRotation;
        this.isShooting = isShooting;
        this.isSwitchColor = isSwitchColor;
        this.mColor = mColor;
    }

    void WriteSignedBinaryValue(OutputMemoryBitStream inOutputStream, float inValue)
    {
        bool isNonZero = (inValue != 0f);
        inOutputStream.Write(isNonZero);
        if (isNonZero)
        {
            inOutputStream.Write(inValue > 0f);
        }
    }

    void ReadSignedBinaryValue(InputMemoryBitStream inInputStream, out float outValue )
    {
        bool isNonZero;
        inInputStream.Read(out isNonZero);
        if (isNonZero)
        {
            bool isPositive;
            inInputStream.Read(out isPositive);
            outValue = isPositive ? 1f : -1f;
        }
        else
        {
            outValue = 0f;
        }
    }
}
