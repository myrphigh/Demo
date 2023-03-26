using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSetController : MonoBehaviour
{
    public Transform []Childs;
    private void Start()
    {
        Childs = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Childs[i] = transform.GetChild(i);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AnimationStart();
         }
    }

    public void AnimationStart()
    {
        for (int i = 0; i < Childs.Length; i++)
        {
            Childs[i].GetComponent<RoomTriggerMethod>().StartAnimation();
        }
    }
}
