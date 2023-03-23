using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLevelControl : MonoBehaviour
{
    public bool trigger = false;
    public EnemySpawner enemySpawner;

    public GameObject[] Enemies;
    private void Start()
    {
        enemySpawner = transform.GetChild(0).GetComponent<EnemySpawner>();
    }
    private void FixedUpdate()
    {
        if (trigger)
        {
            enemySpawner.spawnEnemy(Enemies[0]);
            trigger = false;
        }
    }

    public void spawnOneEnemy()
    {
        trigger = true;
    }
}
