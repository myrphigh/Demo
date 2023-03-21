using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : Singleton<LevelManager>
{
    public static int currentLevelIndex = 0;

    [SerializeField]
    public GameObject[] spawnPointOfEachLevel;
    public static string []scenesSeq;
    public string[] scenesSeqInput;
    public static Vector3[] spawnPointPositions;

    public static GameObject player;
    void Start()
    {
        scenesSeq = scenesSeqInput;
        spawnPointPositions = new Vector3[spawnPointOfEachLevel.Length];
        foreach(GameObject entry in spawnPointOfEachLevel)
        {
            int index;
            int.TryParse(entry.transform.name,out index);
            spawnPointPositions[index] = entry.transform.position;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            player = GameObject.FindWithTag("Player");
            LevelRestart();
        }
    }

    public static  void LevelRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        player.transform.position = spawnPointPositions[currentLevelIndex];
        player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
    }

    public static void NextLevel()
    {
        currentLevelIndex++;
        SceneManager.LoadScene(scenesSeq[currentLevelIndex]);
    }
}
