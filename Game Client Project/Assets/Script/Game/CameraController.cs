using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(-180, 180)]
    public float lookingSpot = 0;
    
    public Transform playerTransform;
    [Range(0, 90)]
    public float LookingAngle = 45;

    public float distanceFromPlayer = 10;
    
    void Start()
    {
        
    }

    void Update()
    {
        if(playerTransform == null) { return; }

        Vector3 offsetDir = Quaternion.Euler(0, lookingSpot, 0) * new Vector3(0, Mathf.Sin(LookingAngle / 90), Mathf.Cos(LookingAngle / 90));

        Vector3 offset = offsetDir.normalized * distanceFromPlayer;

        transform.position = Vector3.Lerp(transform.position, playerTransform.position + offset, 0.5f);

        transform.LookAt(playerTransform.position);
    }
}
