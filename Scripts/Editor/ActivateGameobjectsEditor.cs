using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActivateGameobjects))]
public class ActivateGameobjectsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ActivateGameobjects mytarget = (ActivateGameobjects)target;
        GUIStyle showhide = new GUIStyle(GUI.skin.button);
        showhide.fontSize = 20;
        showhide.fontStyle = FontStyle.Bold;
        if (GUILayout.Button(mytarget.show ? "Hide" : "Show", showhide, GUILayout.Height(50)))
        {
            mytarget.Activate(!mytarget.show);
        }
    }
}