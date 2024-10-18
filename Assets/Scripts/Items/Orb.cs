using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public void ApplyEffect(GameObject player)
    {
        PlayerStats ps = player.GetComponent<PlayerStats>();
        ps.orbsCollected++;
    }
}
