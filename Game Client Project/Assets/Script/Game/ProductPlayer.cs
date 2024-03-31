using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductPlayer : ProductCommon
{
    public Vector3 Velocity { get; set; }
    public float mMaxLinearSpeed;
    public float mMaxRotationSpeed;

    //bounce fraction when hitting various things
    public float mWallRestitution;
    public float mCatRestitution;

    float mLastMoveTimestamp;
    
    float mWorldDirX;
    float mWorldDirZ;
    int mHealth;
    bool mIsShooting;

    Renderer render;

    public int PlayerId { get; set; }

    float mTimeLocationBecameOutOfSync;
    float mTimeVelocityBecameOutOfSync;

    enum EPLERReplicationState
    {
        ECRS_Pose = 1 << 0,
        ECRS_Color = 1 << 1,
        ECRS_PlayerId = 1 << 2,
        ECRS_Health = 1 << 3,

        ECRS_AllState = ECRS_Pose | ECRS_Color | ECRS_PlayerId | ECRS_Health
    };

    private ThirdPersonMovement movement;

    private void Start()
    {
        movement = this.GetComponent<ThirdPersonMovement>();
    }

    public override void Initialize()
    {
        render = transform.GetChild(0).GetComponent<Renderer>();
    }

    public uint GetAllStateMask() { return (uint)EPLERReplicationState.ECRS_AllState; }

    void SetPlayerId(uint inPlayerId) { PlayerId = (int)inPlayerId; }

    public override uint Write(OutputMemoryBitStream inOutputStream, uint inDirtyState)
    {
        // Client doesn't need to write anything here
        throw new System.NotImplementedException();
    }

    public override void Read(InputMemoryBitStream inInputStream)
    {
        bool stateBit;

        uint readState = 0;

        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            uint playerId;
            inInputStream.Read(out playerId);
            SetPlayerId(playerId);
            readState |= (uint)EPLERReplicationState.ECRS_PlayerId;
        }

        float oldRotation = GetRotation();
        Vector3 oldLocation = GetLocation();
        Vector3 oldVelocity = Velocity;

        float replicatedRotation;
        Vector3 replicatedLocation = Vector3.zero;
        Vector3 replicatedVelocity = Vector3.zero;

        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            inInputStream.Read(out replicatedVelocity.x);
            inInputStream.Read(out replicatedVelocity.z);

            Velocity = replicatedVelocity;

            inInputStream.Read(out replicatedLocation.x);
            inInputStream.Read(out replicatedLocation.z);

            SetLocation(replicatedLocation);

            inInputStream.Read(out replicatedRotation);
            SetRotation(replicatedRotation);

            readState |= (uint)EPLERReplicationState.ECRS_Pose;
        }

        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            inInputStream.Read(out mWorldDirX);
            inInputStream.Read(out mWorldDirZ);
        }
        else
        {
            mWorldDirX = 0.0f;
            mWorldDirZ = 0.0f;
        }

        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            byte color;
            inInputStream.Read(out color);
            SetColor(color);
            readState |= (uint)EPLERReplicationState.ECRS_Color;
        }

        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            mHealth = 0;
            inInputStream.Read(out mHealth, 4);
            readState |= (uint)EPLERReplicationState.ECRS_Health;
        }

        if (PlayerId == ((NetworkClient)NetworkClient.Instance).GetPlayerId())   // MainPlayer
        {
            //did we get health? if so, tell the hud!
            if ((readState & (uint)EPLERReplicationState.ECRS_Health) != 0)
            {
                //HUD::sInstance->SetPlayerHealth(mHealth);
            }

            #region Chapter8
            DoClientSidePredictionAfterReplicationForLocalCat(readState);

            //if this is a create packet, don't interpolate
            if ((readState & (uint)EPLERReplicationState.ECRS_PlayerId) == 0)
            {
                InterpolateClientSidePrediction(oldRotation, oldLocation, oldVelocity, false);
            }
            #endregion


            if (Camera.main.GetComponent<CameraController>().playerTransform != this.transform)
            {
                Camera.main.GetComponent<CameraController>().playerTransform = this.transform;
            }
            if(InputManager.Instance.MainPlayer == null)
            {
                InputManager.Instance.MainPlayer = this.transform;
            }
        }

        if (movement == null)
        {
            movement = GetComponent<ThirdPersonMovement>();
        }
        movement.enabled = PlayerId == ((NetworkClient)NetworkClient.Instance).GetPlayerId();

        #region Chapter8
        DoClientSidePredictionAfterReplicationForRemoteCat(readState);

        //if this is a create packet, don't interpolate
        if ((readState & (uint)EPLERReplicationState.ECRS_PlayerId) == 0)
        {
            InterpolateClientSidePrediction(oldRotation, oldLocation, oldVelocity, false);
        }
        #endregion
    }

    void DoClientSidePredictionAfterReplicationForLocalCat(uint inReadState)
    {
        if ((inReadState & (uint)EPLERReplicationState.ECRS_Pose) != 0)
        {
            //simulate pose only if we received new pose- might have just gotten thrustDir
            //in which case we don't need to replay moves because we haven't warped backwards

            //all processed moves have been removed, so all that are left are unprocessed moves
            //so we must apply them...
            MoveList moveList = InputManager.Instance.GetMoveList();

            foreach (var move in moveList.mMoves)
            {
                float deltaTime = move.GetDeltaTime();

                SimulateMovement(deltaTime);
            }

        }

    }

    public void SimulateMovement(float inDeltaTime)
    {
        SetLocation(GetLocation() + Velocity * inDeltaTime);
    }


    void InterpolateClientSidePrediction(float inOldRotation, Vector3 inOldLocation, Vector3 inOldVelocity, bool inIsForRemoteCat )
    {
	    if(inOldRotation - GetRotation() > 0.1f && !inIsForRemoteCat )
	    {
            Debug.Log( "ERROR! Move replay ended with incorrect rotation!");
        }

        float roundTripTime = (NetworkClient.Instance as NetworkClient).GetRoundTripTime();

        if (inOldLocation != GetLocation())
        {
            //LOG( "ERROR! Move replay ended with incorrect location!", 0 );

            //have we been out of sync, or did we just become out of sync?
            float time = Time.time;
            if (mTimeLocationBecameOutOfSync == 0.0f)
            {
                mTimeLocationBecameOutOfSync = time;
            }

            float durationOutOfSync = time - mTimeLocationBecameOutOfSync;
            if (durationOutOfSync < roundTripTime)
            {
                SetLocation(Vector3.Lerp(inOldLocation, GetLocation(), inIsForRemoteCat ? (durationOutOfSync / roundTripTime) : 0.1f));
            }
        }
        else
        {
            //we're in sync
            mTimeLocationBecameOutOfSync = 0.0f;
        }


        if (inOldVelocity != Velocity)
        {
            //LOG( "ERROR! Move replay ended with incorrect velocity!", 0 );

            //have we been out of sync, or did we just become out of sync?
            float time = Time.time;
            if (mTimeVelocityBecameOutOfSync == 0.0f)
            {
                mTimeVelocityBecameOutOfSync = time;
            }

            //now interpolate to the correct value...
            float durationOutOfSync = time - mTimeVelocityBecameOutOfSync;
            if (durationOutOfSync < roundTripTime)
            {
                Velocity = Vector3.Lerp(inOldVelocity, Velocity, inIsForRemoteCat ? (durationOutOfSync / roundTripTime) : 0.1f);
            }
            //otherwise, fine...

        }
        else
        {
            //we're in sync
            mTimeVelocityBecameOutOfSync = 0.0f;
        }
	
    }


    //so what do we want to do here? need to do some kind of interpolation...

    void DoClientSidePredictionAfterReplicationForRemoteCat(uint inReadState)
    {
        if ((inReadState & (uint)EPLERReplicationState.ECRS_Pose) != 0)
        {

            //simulate movement for an additional RTT
            float rtt = (NetworkClient.Instance as NetworkClient).GetRoundTripTime();
            //LOG( "Other cat came in, simulating for an extra %f", rtt );

            //let's break into framerate sized chunks though so that we don't run through walls and do crazy things...
            float deltaTime = 1.0f / 30.0f;

            while (true)
            {
                if (rtt < deltaTime)
                {
                    SimulateMovement(rtt);
                    break;
                }
                else
                {
                    SimulateMovement(deltaTime);
                    rtt -= deltaTime;
                }
            }
        }
    }


public override void SetColor(byte inColor)
    {
        base.SetColor(inColor);


        foreach (ColorCollection color in Enum.GetValues(typeof(ColorCollection)))
        {
            if(inColor == (uint)color)
            {
                string name = color.ToString() + "_1";
                render.material = Resources.Load(name, typeof(Material)) as Material;
            }
        }
    }
}
