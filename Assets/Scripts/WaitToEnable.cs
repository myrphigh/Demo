using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitToEnable : MonoBehaviour
{
    public GameObject targetObject;
    public float waitTime = 1.0f;

    public void OnEnable()
    {
        targetObject.gameObject.SetActive(true);
    }
}
