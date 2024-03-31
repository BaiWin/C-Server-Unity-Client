using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductProjectile : ProductCommon
{
    public Vector3 Velocity { get; set; }
    public int PlayerId { get; set; }

    private float mLifeTime = 1f;

    TrailRenderer trailRenderer;

    enum PRTLReplicationState
    {
        PRTL_Pose = 1 << 0,
        PRTL_Color = 1 << 1,
        PRTL_PlayerId = 1 << 2,

        PRTL_AllState = PRTL_Pose | PRTL_Color | PRTL_PlayerId
    };

    void Update()
    {
        transform.position += Velocity * Time.deltaTime;
        mLifeTime -= Time.deltaTime;
        if(mLifeTime < 0)
        {
            pooledSelf.OnRelease();
        }
    }


    public override void Initialize()
    {
        mLifeTime = 1f;
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public override void Read(InputMemoryBitStream inInputStream)
    {
        bool stateBit;

        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            Vector3 location = Vector3.zero ;
            inInputStream.Read(out location.x);
            inInputStream.Read(out location.z);
            SetLocation(location);

            Vector3 velocity = Vector3.zero;
            inInputStream.Read(out velocity.x);
            inInputStream.Read(out velocity.z);
            Velocity = velocity;

            float rotation;
            inInputStream.Read(out rotation);
            SetRotation(rotation);
        }


        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            byte color;
            inInputStream.Read(out color);
            SetColor(color);
        }

        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            int playerid = 0;
            inInputStream.Read(out playerid, 8);
            PlayerId = playerid;
        }
    }

    public override uint Write(OutputMemoryBitStream inOutputStream, uint inDirtyState)
    {
        uint writtenState = 0;

        if ((inDirtyState & (uint)PRTLReplicationState.PRTL_Pose) != 0)
        {
            inOutputStream.Write((bool)true);

            Vector3 location = GetLocation();
            inOutputStream.Write(location.x);
            inOutputStream.Write(location.z);

            Vector3 velocity = Velocity;
            inOutputStream.Write(velocity.x);
            inOutputStream.Write(velocity.z);

            inOutputStream.Write(GetRotation());

            writtenState |= (uint)PRTLReplicationState.PRTL_Pose;
        }
        else
        {
            inOutputStream.Write((bool)false);
        }

        if ((inDirtyState & (uint)PRTLReplicationState.PRTL_Color) != 0)
        {
            inOutputStream.Write((bool)true);

            inOutputStream.Write(GetColor());

            writtenState |= (uint)PRTLReplicationState.PRTL_Color;
        }
        else
        {
            inOutputStream.Write((bool)false);
        }

        if ((inDirtyState & (uint)PRTLReplicationState.PRTL_PlayerId) != 0)
        {
            inOutputStream.Write((bool)true);

            inOutputStream.Write(PlayerId, 8);

            writtenState |= (uint)PRTLReplicationState.PRTL_PlayerId;
        }
        else
        {
            inOutputStream.Write((bool)false);
        }
        return writtenState;
    }

    public override void SetColor(byte inColor)
    {
        base.SetColor(inColor);

        foreach (ColorCollection color in Enum.GetValues(typeof(ColorCollection)))
        {
            if (inColor == (uint)color)
            {
                string name = color.ToString() + "_1";
                trailRenderer.material = Resources.Load(name, typeof(Material)) as Material;
            }
        }
        
    }
}
