using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    public float speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = FindObjectOfType<CharacterController>().gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Time.deltaTime * speed * Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += Time.deltaTime * speed * Vector3.left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += Time.deltaTime * speed * Vector3.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position -= Time.deltaTime * speed * Vector3.forward;
        }
    }
}
