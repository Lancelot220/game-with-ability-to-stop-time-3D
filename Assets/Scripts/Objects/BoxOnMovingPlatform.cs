using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxOnMovingPlatform : MonoBehaviour
{
    bool playerOnTop;
    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Player"))
        {
            col.transform.SetParent(transform);

        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.collider.CompareTag("Player"))
        {
            col.transform.SetParent(null);
            StartCoroutine(ResetRotation(col.transform));
        }
    }

    IEnumerator ResetRotation(Transform player)
    {
        while (player.rotation != Quaternion.Euler(0, player.eulerAngles.y, 0))
        {
            player.rotation = Quaternion.Lerp(player.rotation, Quaternion.Euler(0, player.eulerAngles.y, 0), Time.deltaTime * 5f);
            yield return null;
        }
    }
}
