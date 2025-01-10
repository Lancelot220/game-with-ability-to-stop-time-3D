using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Tooltip("At least two points.")]
    public Transform[] points;
    public bool startPointAsPoint1;
    public bool loop = true;
    [Tooltip("Set to true if after reaching the last point the object should go to first point. Set to false if after reaching the last point the obejct should go back to first point through all other points.")]
    public bool lap;
    public float speed = 1;
    [Tooltip("For infinite moving in one direction.")]
    public bool instantReturning;

    Vector3 currentDest;
    int currentDestIndex;
    bool movingTowardsPoint1;
    bool reversed;

    void Start()
    {
        if(startPointAsPoint1) points[0] = transform;
        currentDest = points[1].position;
        currentDestIndex = 1;
    }
    void Update()
    {
        if (Vector3.Distance(currentDest, transform.position) > 0.1f) //moving towards point
        {
            transform.position = Vector3.MoveTowards(transform.position, currentDest, speed);
        }
        else //switch to next point
        {
            if(!reversed)
            {
                currentDestIndex++;
                if(currentDestIndex + 1 > points.Length) //what to do if reached the last point
                {
                    if(loop)
                    {
                        if(lap) currentDestIndex = 0; 
                        else { reversed = true; currentDestIndex--; }
                    }
                }
            }
            else
            {
                currentDestIndex--;
                if(currentDestIndex < 0) //what to do if reached first point
                {
                    currentDestIndex = 1;
                    reversed = false;
                }
            }
            //and finaly...
            currentDest = points[currentDestIndex].position;
        }
    }
}
