using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Tooltip("At least two points. If Start Point As Point 1 is set to true, leave the first element empty.")]
    public Transform[] points;
    [Tooltip("1 means first point, which is actually the element 0.")]
    public bool startPointAsPoint1; 
    public bool loop = true;
    [Tooltip("Set to true if after reaching the last point the object should go to first point. Set to false if after reaching the last point the obejct should go back to first point through all other points.")]
    public bool lap;
    public float speed = 1;
    [Tooltip("For infinite moving in one direction.")]
    public bool instantReturning;

    Vector3 currentDest;
    int currentDestIndex;
    bool reversed;
    bool pointReached;

    void Start()
    {
        if(startPointAsPoint1)
        {   
            GameObject firstPoint = new GameObject("First Point");
            firstPoint.transform.position = transform.position;
            points[0] = firstPoint.transform;
        }
        currentDest = points[1].position;
        currentDestIndex = 1;
    }
    void Update()
    {
        if (Vector3.Distance(currentDest, transform.position) > 0.1f) //moving towards point
        {
            transform.position = Vector3.MoveTowards(transform.position, currentDest, speed * Time.deltaTime);
            pointReached = false;
        }
        else if(!pointReached)//switch to next point
        {
            pointReached = true;
            if(!reversed)
            {
                currentDestIndex++;
                if(currentDestIndex + 1 > points.Length) //what to do if reached the last point
                {
                    if(loop)
                    {
                        if(lap) currentDestIndex = 0; 
                        else { reversed = true; currentDestIndex -= 2; }
                    }
                    else currentDestIndex--;
                }
            }
            else
            {
                currentDestIndex--;
                if(currentDestIndex < 0) //what to do if reached first point
                {
                    if(loop)
                    {
                        currentDestIndex = 1;
                        reversed = false;
                    }
                    else
                    {
                        currentDestIndex = 0;
                    }
                }
            }
            //and finaly...
            currentDest = points[currentDestIndex].position;
        }

        if(currentDestIndex == 0 && instantReturning) //fast returning to first point
        {
            transform.position = points[0].position;
            pointReached = false;
        }
    }
}
