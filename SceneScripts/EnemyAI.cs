using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

    [SerializeField] Transform[] waypoints;
    int waypointIndex;
    Vector3 target;
    [SerializeField] float speed;
    // Start is called before the first frame update
    void Start()
    {
        target = waypoints[0].position; // set the initial target position to the first waypoint
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != waypoints[waypointIndex].position) {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[waypointIndex].position, speed * Time.deltaTime);
            Vector3 direction = waypoints[waypointIndex].position - transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
        }
        else {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }
    }
}
