using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndReceiver : MonoBehaviour
{
    Vector3 defaultModelPos;
    void Start() {defaultModelPos = transform.localPosition;}
    void AttackEnd()
    { GameObject.Find("Player").GetComponentInChildren<Attack2>().AttackEnd(); }

    void LedgeClimbEnd()
    { GameObject.Find("Player").GetComponent<Movement>().EndClimbing(gameObject.transform, defaultModelPos, transform.localPosition); }
}
