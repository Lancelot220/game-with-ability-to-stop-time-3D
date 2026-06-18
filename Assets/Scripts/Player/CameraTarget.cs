using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public float yFollowSpeed = .1f;
    public float yFollowSpeedFaster = .1f;
    public float distanceFromSurface = .1f;
    public Transform player;
    Movement movement;
    // Start is called before the first frame update
    void Start()
    {
        movement = FindObjectOfType<Movement>();
        player = movement.transform;
        gameObject.transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        if(!movement.onGround)
        transform.position = new Vector3(player.position.x, Mathf.Lerp(transform.position.y, player.position.y, yFollowSpeed), player.position.z);
        else if(movement.rb.velocity.y < 0)
        transform.position = new Vector3(player.position.x, Mathf.Lerp(transform.position.y, player.position.y, yFollowSpeedFaster), player.position.z);
            else
        transform.position = new Vector3(player.position.x, Mathf.Lerp(transform.position.y, player.position.y, yFollowSpeed), player.position.z);

        Physics.Raycast(player.position, -transform.up, out RaycastHit hit, Vector3.Distance(player.position, transform.position), LayerMask.GetMask("Default", "Obstacles"));
        if (hit.collider != null)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + distanceFromSurface, transform.position.z);
        }
    }

    // void OnTriggerStay(Collider col)
    // {
    //     print("its working");
    //     if (col.includeLayers == LayerMask.GetMask("Default") || col.includeLayers == LayerMask.GetMask("Obstacles"))
    //     {
    //         Physics.Raycast(new Ray(transform.position, transform.up), out RaycastHit hit, LayerMask.GetMask("Default", "Obstacles"));
    //             if (hit.collider != null)
    //             {
    //                 transform.position = new Vector3(transform.position.x, hit.point.y + distanceFromSurface, transform.position.z);
    //             }
    //     }
    // }
}
