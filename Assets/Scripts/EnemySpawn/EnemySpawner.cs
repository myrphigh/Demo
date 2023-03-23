using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject fromPoint;
    public GameObject toPoint;
    public float speedInSpawning = 1f;
    Transform tempFrom;
    Transform tempTo;
    GameObject spawnTemp;

    Vector3 direction;
    void Start()
    {
        tempFrom = fromPoint.transform;
        tempTo = toPoint.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawnEnemy(GameObject spawnTarget)
    {
        GameObject temp = Instantiate(spawnTarget, fromPoint.transform);
        spawnTemp = temp;
        temp.transform.position = fromPoint.transform.position;
        //direction = tempTo.position - tempFrom.position;
        //StartCoroutine("moveEnemy");
    }

    IEnumerator moveEnemy()
    {
        while(Vector3.Distance(spawnTemp.transform.position,tempTo.transform.position) > 0.5f) 
        {
            Debug.Log("in corotine");
            spawnTemp.transform.Translate(direction * speedInSpawning * Time.deltaTime);
            yield return new WaitForFixedUpdate(); 
        }
        StopCoroutine("moveEnemy");

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(fromPoint.transform.position, .3f);
        Gizmos.DrawSphere(toPoint.transform.position, .3f);
        Gizmos.DrawLine(fromPoint.transform.position, toPoint.transform.position);
    }
}
