using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : IMemoryStream
{
    InputState mInputState;
    float mTimestamp;
    float mDeltaTime;

    public Move(InputState mInputState, float mTimestamp, float mDeltaTime)
    {
        this.mInputState = mInputState;
        this.mTimestamp = mTimestamp;
        this.mDeltaTime = mDeltaTime;
    }

    public InputState GetInputState() { return mInputState; }
    public float GetTimestamp() { return mTimestamp; }
    public float GetDeltaTime() { return mDeltaTime; }

    public void Read(InputMemoryBitStream inInputStream)
    {
        mInputState.Read(inInputStream);
        inInputStream.Read(out mTimestamp);
    }

    public void Write(OutputMemoryBitStream inOutputStream)
    {
        mInputState.Write(inOutputStream);
        inOutputStream.Write(mTimestamp);
    }
}
