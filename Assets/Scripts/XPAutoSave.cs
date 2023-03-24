using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;

[InitializeOnLoad]
public class XPAutoSave
{

    public static Scene nowScene;
    public static DateTime lastSaveTime = DateTime.Now;
    static XPAutoSave()
    {
        lastSaveTime = DateTime.Now;
        EditorApplication.update += EditorUpdate;
    }
    ~XPAutoSave()
    {
        EditorApplication.update -= EditorUpdate;
    }
    static void EditorUpdate()
    {
        if (AutoSaveWindow.autoSaveScene)
        {
            double seconds = (DateTime.Now - lastSaveTime).TotalSeconds;
            if (seconds > AutoSaveWindow.intervalTime)
            {
                saveScene();
                lastSaveTime = DateTime.Now;
            }
        }
    }

    static void saveScene()
    {
        if (nowScene.isDirty)
        {
            nowScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(nowScene);
            if (AutoSaveWindow.showMessage)
            {
                Debug.Log("自动保存场景: " + nowScene.path + "  " + lastSaveTime);
            }
        }

    }
}

