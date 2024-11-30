using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.Util;

public class Breakables : MonoBehaviour
{
    [Tooltip("Add the Orb as 0 element (if it should be dropped).")] 
    public GameObject[] drop;
    [Range(0, 100)] public int minOrbCount;
    [Range(0, 100)] public int maxOrbCount;
    public UnityEvent onBreak;
    
    public void Break()
    {
        if(drop.Length > 0)
        {
            if(drop[0].GetComponent<Orb>() != null)
            {
                for (int i = 0; i < Random.Range(minOrbCount, maxOrbCount + 1); i++)
                {
                    GameObject orb = Instantiate(drop[0], transform.position, Quaternion.identity);
                    orb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1f,1f), Random.Range(0.01f,1f), Random.Range(-1f,1f)));
                }
            }
            else
            {
                foreach (GameObject item in drop)
                {
                    Instantiate(item, transform.position, Quaternion.identity);
                }
            }
        }

        onBreak.Invoke();

        Destroy(gameObject);
    }
}
