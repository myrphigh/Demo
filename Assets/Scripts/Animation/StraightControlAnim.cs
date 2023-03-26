using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightControlAnim : MonoBehaviour
{
    public GameObject[] AnimTargets;
    public bool notActed = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&& notActed)
        {
            StartCoroutine(StartAnimGroupBySec());
            notActed = false;
        }
    }
    IEnumerator StartAnimGroupBySec()
    {
        for (int i = 0; i < AnimTargets.Length; i++)
        {
            AnimTargets[i].GetComponent<AnimSetController>().AnimationStart();
            yield return new WaitForSeconds(0.4f);
        }
    }
}
