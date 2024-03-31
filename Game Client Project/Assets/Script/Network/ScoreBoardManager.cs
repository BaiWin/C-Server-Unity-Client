using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreBoardManager : MonoSingleton<ScoreBoardManager>, IMemoryStream
{
    List<Entry> mEntries = new List<Entry>();

    List<Vector3> mDefaultColors;

    public Entry GetEntry(uint inPlayerId)
    {
        foreach (var entry in mEntries)
        {
            if (entry.GetPlayerId() == inPlayerId)
            {
                return entry;
            }
        }
        return null;
    }
    public bool RemoveEntry(uint inPlayerId)
    {
        foreach(var entry in mEntries)
        {
            if (entry.GetPlayerId() == inPlayerId)
            {
                mEntries.Remove(entry);
                return true;
            }
        }
        return false;
    }
    public void AddEntry(uint inPlayerId, string inPlayerName)
    {
        //if this player id exists already, remove it first- it would be crazy to have two of the same id
        RemoveEntry(inPlayerId);

        mEntries.Add(new Entry(inPlayerId, inPlayerName, mDefaultColors[(int)inPlayerId % mDefaultColors.Count]));
    }
	public void IncScore(uint inPlayerId, int inAmount)
    {
        Entry entry = GetEntry(inPlayerId);
        if (entry != null)
        {
            entry.SetScore(entry.GetScore() + inAmount);
        }
    }

    public void Read(InputMemoryBitStream inInputStream)
    {
        int entryCount;
        inInputStream.Read(out entryCount);
        //just replace everything that's here, it don't matter...
        Helper.Resize<Entry>(mEntries, entryCount);
        foreach (var entry in mEntries)
        {
            entry.Read(inInputStream);
        }
    }

    public void Write(OutputMemoryBitStream inOutputStream)
    {
        int entryCount = mEntries.Count;

        //we don't know our player names, so it's hard to check for remaining space in the packet...
        //not really a concern now though
        inOutputStream.Write(entryCount);
        foreach( var entry in mEntries)
	    {
            entry.Write(inOutputStream);
        }
    }

    public List<Entry> GetEntries() { return mEntries; }

    public class Entry : IMemoryStream
    {
        Vector3 mColor;
        uint mPlayerId;
        string mPlayerName;
        int mScore;
        string mFormattedNameScore;

        public Entry()
        {
            mPlayerName = "";
            mFormattedNameScore = "";
        }

        public Entry(uint inPlayerId, string inPlayerName, Vector3 inColor )
        {
            mPlayerId = inPlayerId;
            mPlayerName = inPlayerName;
            mColor = inColor;
            SetScore(0);
        }

        public Vector3 GetColor() { return mColor; }
        public uint GetPlayerId() { return mPlayerId; }
        public string GetPlayerName() { return mPlayerName; }
        public string GetFormattedNameScore() { return mFormattedNameScore; }
        public int GetScore() { return mScore; }

        public void SetScore(int inScore)
        {
            mScore = inScore;

            mFormattedNameScore = string.Format("{0} : {1}", mPlayerName, inScore);
        }

        public void Read(InputMemoryBitStream inInputStream)
        {
            bool didSucceed = true;

            inInputStream.Read(out mColor);
            inInputStream.Read(out mPlayerId);
            mPlayerName = "";
            string iamnew = "";
            inInputStream.Read(out iamnew);
            mPlayerName = iamnew;

            int score;
            inInputStream.Read(out score);
            if (didSucceed)
            {
                SetScore(score);
            }
        }

        public void Write(OutputMemoryBitStream inOutputStream)
        {
            //bool didSucceed = true;

            inOutputStream.Write(mColor);
            inOutputStream.Write(mPlayerId);
            inOutputStream.Write(mPlayerName);
            inOutputStream.Write(mScore);
        }
    }
}
