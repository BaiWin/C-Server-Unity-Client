using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    private CharacterController controller;

    public float speed = 2f;

    const float HALF_WORLD_CIRCLE_RADIUS = 13.0f;

    const float collisionRadius = 0.5f;

    Vector3 r;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            Vector3 fromCamera = transform.position - Camera.main.transform.position;
            Vector3 cameraForward = new Vector3(fromCamera.x, 0, fromCamera.z).normalized;
            direction = rotation * cameraForward;
            transform.position += direction * speed * Time.deltaTime;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100))
        {
            Vector3 fromMouse = hit.point - transform.position;
            Vector3 mouseForward = new Vector3(fromMouse.x, 0, fromMouse.z).normalized;

            float angle = Vector3.SignedAngle(Vector3.forward, mouseForward, Vector3.up);          
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        r = hit.point;
    }


    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    if (r != null)
    //    {
    //        Gizmos.DrawSphere(r, 10);
    //    }
    //}
}
