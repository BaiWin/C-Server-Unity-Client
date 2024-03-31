using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ReplicationCommand;
using static UnityEngine.PlayerLoop.PreUpdate;

public enum ReplicationAction
{
    RA_Create,
    RA_Update,
    RA_Destroy,
    RA_RPC,
    RA_MAX
};

public class ReplicationCommand
{
    public ReplicationCommand(uint inInitialDirtyState)
    {
        this.mDirtyState = inInitialDirtyState;
        this.mAction = ReplicationAction.RA_Create;
    }

    uint mDirtyState;
    ReplicationAction mAction;

    void HandleCreateAckd() { if (mAction == ReplicationAction.RA_Create) { mAction = ReplicationAction.RA_Update; } }
    void AddDirtyState(uint inState) { mDirtyState |= inState; }
    void SetDestroy() { mAction = ReplicationAction.RA_Destroy; }

    bool HasDirtyState() { return (mAction == ReplicationAction.RA_Destroy ) || (mDirtyState != 0 ); }

    void SetAction(ReplicationAction inAction) { mAction = inAction; }
    ReplicationAction GetAction() { return mAction; }
	uint GetDirtyState() { return mDirtyState; }

    void ClearDirtyState(uint inStateToClear)
    {
        mDirtyState &= ~inStateToClear;

        if (mAction == ReplicationAction.RA_Destroy)
        {
            mAction = ReplicationAction.RA_Update;
        }
    }
}
