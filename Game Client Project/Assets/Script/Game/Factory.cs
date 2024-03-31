using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProduct
{
    string ProductName { get; set; }

    void Initialize();
}

public abstract class Factory : MonoSingleton<Factory>
{
    public abstract IProduct GetProduct(string productName, Vector3 position);

    // shared method with all factories
 
}
