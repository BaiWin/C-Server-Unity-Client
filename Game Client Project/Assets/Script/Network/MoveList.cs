using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveList : MySingleton<MoveList>
{
    float mLastMoveTimestamp;

    public List<Move> mMoves = new List<Move>();

    public MoveList()
    {
        this.mLastMoveTimestamp = -1f;
    }

    public Move AddMove(InputState inInputState, float inTimestamp)
    {
        float deltaTime = Time.deltaTime;

        mMoves.Add(new Move(inInputState, inTimestamp,  deltaTime));

        mLastMoveTimestamp = inTimestamp;

        return mMoves[mMoves.Count - 1];
    }

    public void RemovedProcessedMoves(float inLastMoveProcessedOnServerTimestamp)
    {
        while (mMoves.Count > 0 && mMoves[0].GetTimestamp() <= inLastMoveProcessedOnServerTimestamp)
        {
            mMoves.RemoveAt(0);
        }
    }

    public float GetLastMoveTimestamp() { return mLastMoveTimestamp; }

    public Move GetLatestMove() { return mMoves[mMoves.Count - 1]; }

    public void Clear() { mMoves.Clear(); }

    public bool HasMoves() { return mMoves.Count > 0; }
    public int GetMoveCount() { return mMoves.Count; }

    public Move GetAtIndex(int index) { return mMoves[index]; }
}
