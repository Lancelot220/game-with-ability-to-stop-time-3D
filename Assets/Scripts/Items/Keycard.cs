using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keycard : MonoBehaviour
{
    public GameObject doors;
    public void OpenDoors()
    {
        doors.GetComponent<Door>().condition = true;
    }
}
