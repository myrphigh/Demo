using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindLevelManager : MonoBehaviour
{
    public static LevelManager LM;
    void Start()
    {
        LM = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
    }

    public void TryNextLevel()
    {
        Debug.Log("try load next level");
        if(LM != null)
        {
            LM.tryNext();
        }
    }
}
