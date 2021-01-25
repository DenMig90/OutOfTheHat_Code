using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LocalizationObject))]
public class LocalizationObjCustomInspector : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LocalizationObject localization = (LocalizationObject)target;
        GUIStyle labelstyle = new GUIStyle();
        labelstyle.fontStyle = FontStyle.Italic;
        GUIStyle errorStyle = new GUIStyle();
        errorStyle.normal.textColor = Color.red;
        if (LocalizationManager.instance == null)
        {
            EditorGUILayout.LabelField("There is no LocalizationManager in scene", errorStyle);
        }
        EditorGUILayout.LabelField("It's recommended to press Reset before editing", labelstyle);

        GUILayout.BeginHorizontal();

        GUIStyle buttonstyle = new GUIStyle(GUI.skin.button);
        buttonstyle.fontSize = 20;
        buttonstyle.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Reset", buttonstyle, GUILayout.Height(50)))
        {
            localization.OnLoadPackage();
        }
        if (GUILayout.Button("Overwrite", buttonstyle, GUILayout.Height(50)))
        {
            localization.OnSavePackage();
            LocalizationManager.instance.SavePackages();
        }
        GUILayout.EndHorizontal();
        EditorUtility.SetDirty(target);
    }

}
