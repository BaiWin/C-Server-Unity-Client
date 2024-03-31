using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timing : MonoSingleton<Timing>
{
    float mTime;
    float mDeltaTime;
    Int64 mDeltaTick;

    double mLastFrameStartTime;
    float mFrameStartTimef;
    double mPerfCountDuration;

    public float GetTime()
    {
        return mTime;
    }

    private void Start()
    {
        mTime = Time.time;
    }

    void Update()
    {
        mTime = Time.time;
    }
}
