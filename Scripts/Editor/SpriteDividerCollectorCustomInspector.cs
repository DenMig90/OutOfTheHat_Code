using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteDividerCollector))]
public class SpriteDividerCollectorCustomInspector : Editor
{
    private bool buttonPressed;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SpriteDividerCollector mytarget = (SpriteDividerCollector)target;
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Cancel", style))
        {
            mytarget.CancelDividing();
            buttonPressed = false;
        }
        EditorGUI.BeginDisabledGroup(mytarget.routine != null);
        if (GUILayout.Button("Divide", style))
        {
            mytarget.StartDividingAll();
            buttonPressed = true;
        }
        if (mytarget.actual < mytarget.target && buttonPressed)
            EditorUtility.DisplayProgressBar("Sprite GameObject Division...", "(" + mytarget.actual + "/" + mytarget.target + ")", (float)mytarget.actual / (float)mytarget.target);
        else
        {
            EditorUtility.ClearProgressBar();
            buttonPressed = false;
        }
        EditorGUI.EndDisabledGroup();
        EditorUtility.SetDirty(mytarget.gameObject);
    }
}