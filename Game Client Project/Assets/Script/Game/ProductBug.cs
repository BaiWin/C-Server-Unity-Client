using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductBug : ProductCommon
{
    enum EMouseReplicationState
    {
        EMRS_Pose = 1 << 0,
        EMRS_Color = 1 << 1,

        EMRS_AllState = EMRS_Pose | EMRS_Color
    };

    Renderer render;

    public override void Initialize()
    {
        render = transform.GetChild(0).GetComponent<Renderer>();
    }

    public uint GetAllStateMask() { return (uint)EMouseReplicationState.EMRS_AllState; }

    public override uint Write(OutputMemoryBitStream inOutputStream, uint inDirtyState)
    {
        uint writtenState = 0;

        if ((inDirtyState & (uint)EMouseReplicationState.EMRS_Pose) != 0)
        {
            inOutputStream.Write((bool)true);

            Vector3 location = GetLocation();
            inOutputStream.Write(location.x);
            inOutputStream.Write(location.z);

            inOutputStream.Write(GetRotation());

            writtenState |= (uint)EMouseReplicationState.EMRS_Pose;
        }
        else
        {
            inOutputStream.Write((bool)false);
        }

        if ((inDirtyState & (uint)EMouseReplicationState.EMRS_Pose) != 0)
        {
            inOutputStream.Write((bool)true);

            inOutputStream.Write(GetColor());

            writtenState |= (uint)EMouseReplicationState.EMRS_Color;
        }
        else
        {
            inOutputStream.Write((bool)false);
        }
        return writtenState;
    }
	public override void Read(InputMemoryBitStream inInputStream)
    {
        bool stateBit;

        inInputStream.Read(out stateBit);
        if (stateBit)
        {
            Vector3 location = Vector3.zero;
            inInputStream.Read(out location.x);
            inInputStream.Read(out location.z);
            SetLocation(location);

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
    }

    public override void SetColor(byte inColor)
    {
        base.SetColor(inColor);

        foreach (ColorCollection color in Enum.GetValues(typeof(ColorCollection)))
        {
            if (inColor == (uint)color)
            {
                string name = color.ToString() + "_1";
                render.material = Resources.Load(name, typeof(Material)) as Material;
            }
        }
    }
}
