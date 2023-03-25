using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTriggerMethod : MonoBehaviour
{
    public Transform[] Childs;
    void Start()
    {
        Childs = new Transform[transform.childCount];
        for(int i = 0;i < transform.childCount; i++)
        {
            Debug.Log(i);
            Childs[i] = transform.GetChild(i);
            Debug.Log(Childs[i].name);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            StartAnimation();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            StartAnimation();
        }
    }

    public void StartAnimation()
    {
        for (int i = 0; i < Childs.Length; i++)
        {
            GameObject targetGameobject = Childs[i].gameObject;
            targetGameobject.GetComponent<Animator>().Play("Action");
        }
    }

}
