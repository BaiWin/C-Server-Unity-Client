using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcreteBugFactory : Factory
{
    //[SerializeField] private ProductCommon bugPrefab;
    public override IProduct GetProduct(string fourCCName, Vector3 position)
    {
        // create a Prefab instance and get the product component
        PooledGameObject instance = PoolManager.Instance.Spawn(fourCCName);
        ProductCommon newProduct = instance.GetComponent<ProductCommon>();
        newProduct.SetBindToPool(instance);

        // each product contains its own logic
        newProduct.Initialize();

        return newProduct;
        
    }
}
