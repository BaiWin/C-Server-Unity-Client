using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedTimedMovingAverage
{
    float mTimeLastEntryMade;
    float mValue;
    float mDuration;

    public WeightedTimedMovingAverage(float inDuration = 5.0f)
    {
        this.mTimeLastEntryMade = Time.time;
        this.mValue = 0;
        this.mDuration = inDuration;
    }

    public void UpdatePerSecond(float inValue)
    {
        float time = Timing.Instance.GetTime();
        float timeSinceLastEntry = time - mTimeLastEntryMade;

        float valueOverTime = inValue / timeSinceLastEntry;

        float fractionOfDuration = (timeSinceLastEntry / mDuration);
        if( fractionOfDuration > 1.0f) { fractionOfDuration = 1.0f; }

        mValue = mValue * (1.0f - fractionOfDuration) + valueOverTime * fractionOfDuration;

        mTimeLastEntryMade = time;
    }

    public void Update(float inValue)
    {
        float time = Time.time;
        float timeSinceLastEntry = time - mTimeLastEntryMade;

        //now update our value by whatever amount of the duration that was..
        float fractionOfDuration = (timeSinceLastEntry / mDuration);
        if (fractionOfDuration > 1.0f) { fractionOfDuration = 1.0f; }

        mValue = mValue * (1.0f - fractionOfDuration) + inValue * fractionOfDuration;

        mTimeLastEntryMade = time;
    }

    public float GetValue() { return mValue; }
}
