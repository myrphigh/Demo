using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;

public class AutoSaveWindow : EditorWindow
{
    public static bool autoSaveScene = true;
    public static bool showMessage = true;
    public static int intervalTime = 30;
    [MenuItem("XP/�Զ��������")]
    static void Init()
    {
        EditorWindow saveWindow = EditorWindow.GetWindow(typeof(AutoSaveWindow));
        saveWindow.minSize = new Vector2(200, 200);
        saveWindow.Show();
    }
    void OnGUI()
    {
        GUILayout.Label("��Ϣ", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("���泡��:", "" + XPAutoSave.nowScene.path);
        GUILayout.Label("ѡ��", EditorStyles.boldLabel);
        autoSaveScene = EditorGUILayout.BeginToggleGroup("�Զ�����", autoSaveScene);
        intervalTime = EditorGUILayout.IntField("ʱ����(��)", intervalTime);
        EditorGUILayout.EndToggleGroup();
        showMessage = EditorGUILayout.BeginToggleGroup("��ʾ��Ϣ", showMessage);
        EditorGUILayout.EndToggleGroup();
    }
}

