using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bullet;
    public float bulletSpeed = 10;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Play fire animation
        if(Input.GetMouseButtonDown(0))
        {
            //Send input
            GameObject go = Instantiate(bullet, transform.position - transform.forward * 2, Quaternion.identity);
            go.GetComponent<Rigidbody>().AddForce(-transform.forward * bulletSpeed);
        }
    }
}
