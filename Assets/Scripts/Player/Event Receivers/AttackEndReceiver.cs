using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEndReceiver : MonoBehaviour
{
    void AttackEnd()
    { GameObject.Find("Player").GetComponentInChildren<Attack2>().AttackEnd(); }
}
