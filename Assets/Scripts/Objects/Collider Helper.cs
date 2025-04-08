using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderHelper : MonoBehaviour
{
    GameObject player;
    GameObject colliderHelper;
    // Start is called before the first frame update
    void Start()
    {
        colliderHelper = transform.GetChild(0).gameObject;
    }

    void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            player = col.gameObject;
            if(player.GetComponent<Movement>().animator.GetBool("isCrouching")) colliderHelper.transform.position = player.transform.position + new Vector3(0, -1.393258f, 0);
        }
    }
}
