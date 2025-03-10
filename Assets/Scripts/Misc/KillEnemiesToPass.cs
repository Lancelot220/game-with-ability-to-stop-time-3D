using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KillEnemiesToPass : MonoBehaviour
{
    public Enemy[] enemiesToKill;
    public UnityEvent onEnemiesKilled;
    bool enemiesKilled;
    void Update()
    {
        if (!enemiesKilled)
        {
            enemiesKilled = true;
            foreach (var enemy in enemiesToKill)
            {
                if (enemy != null)
                {
                    enemiesKilled = false;
                    break;
                }
            }

            if (enemiesKilled)
            {
                onEnemiesKilled.Invoke();
            }
        }
    }
}
