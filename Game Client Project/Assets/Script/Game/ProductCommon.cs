using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorCollection
{
    Red,
    Blue,
    Green,
    Yellow,
    Pink,
}

public abstract class ProductCommon : PooledGameObject, INetworkObject, IProduct
{
    [SerializeField] private string productName = "ProductA";
    public string ProductName { get => productName; set => productName = value; }

    [field: SerializeField]
    public int mNetworkId { get; set; }

    bool mDoesWantToDie;

    byte mColor;

    public PooledGameObject pooledSelf;

    public abstract void Initialize();

    public bool DoesWantToDie() { return mDoesWantToDie; }
    public void SetDoesWantToDie(bool inWants) { mDoesWantToDie = inWants; }

    public void SetNetworkId(int inNetworkId)
    {
        mNetworkId = inNetworkId;
    }

    public abstract void Read(InputMemoryBitStream inInputStream);

    public abstract uint Write(OutputMemoryBitStream inOutputStream, uint inDirtyState);

    public void SetBindToPool(PooledGameObject pooledGo)
    {
        pooledSelf = pooledGo;
    }    

    public Vector3 GetLocation()
    {
        return transform.position;
    }

    public float GetRotation()
    {
        return transform.rotation.y;
    }

    public void SetLocation(Vector3 inLocation)
    {
        transform.position = inLocation;
    }

    public void SetRotation(float inRotation)
    {
        transform.rotation = Quaternion.Euler(0, inRotation, 0);
    }

    public byte GetColor()
    {
        return mColor;
    }

    public virtual void SetColor(byte inColor)
    {
        mColor = inColor;
    }
}
