using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public float yFollowSpeed = .1f;
    public float yFollowSpeedFaster = .1f;
    public float distanceFromSurface = .1f;
    public Transform player;
    public float deltaY = 5;
    Movement movement;
    float prevY; //previous Y level
    // Start is called before the first frame update
    void Start()
    {
        movement = FindObjectOfType<Movement>();
        player = movement.transform;
        //gameObject.transform.SetParent(null);

        prevY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        // if(!movement.onGround)
        // {   
            //transform.position = new Vector3(player.position.x, Mathf.Lerp(transform.position.y, player.position.y, yFollowSpeed), player.position.z);

            // Physics.Raycast(player.position, -transform.up, out RaycastHit hit, 5, LayerMask.GetMask("Default", "Obstacles"));
            // if (hit.collider != null)
            // {
            //     transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, hit.point.y + distanceFromSurface, yFollowSpeed), transform.position.z);
            // }

        //}
        // else if(movement.rb.velocity.y < 0)
        // transform.position = new Vector3(player.position.x, Mathf.Lerp(transform.position.y, player.position.y, yFollowSpeedFaster), player.position.z);
        //     else
        // transform.position = new Vector3(player.position.x, Mathf.Lerp(transform.position.y, player.position.y, yFollowSpeed), player.position.z);

        if(Mathf.Abs(transform.position.y - prevY) < deltaY)
        { transform.position = new Vector3(transform.position.x, prevY, transform.position.z);}
        prevY = transform.position.y;

        if(movement.onGround)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, yFollowSpeed);
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
