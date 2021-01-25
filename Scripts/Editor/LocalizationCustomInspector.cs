using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LocalizationManager))]
public class LocalizationCustomInspector : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LocalizationManager localization = (LocalizationManager)target;

        GUILayout.BeginHorizontal();

        GUIStyle buttonstyle = new GUIStyle(GUI.skin.button);
        buttonstyle.fontSize = 20;
        buttonstyle.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Save", buttonstyle, GUILayout.Height(50)))
        {
            localization.SavePackages();
        }
        if (GUILayout.Button("Load", buttonstyle, GUILayout.Height(50)))
        {
            localization.LoadPackages();
        }

        GUILayout.EndHorizontal();
        EditorUtility.SetDirty(target);
    }

}
