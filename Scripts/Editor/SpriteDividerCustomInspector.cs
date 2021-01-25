using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteDivider))]
public class SpriteDividerCustomInspector : Editor
{
    private bool buttonPressed;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SpriteDivider mytarget = (SpriteDivider)target;
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Clear", style))
        {
            mytarget.Clear();
            buttonPressed = false;
        }
        EditorGUI.BeginDisabledGroup(mytarget.size == 0);
        if (GUILayout.Button("Divide", style))
        {
            mytarget.Clear();
            mytarget.StartDivide();
            buttonPressed = true;
        }
        if (mytarget.actual < mytarget.target && buttonPressed)
            EditorUtility.DisplayProgressBar("Sprite Division...", "(" + mytarget.actual + "/" + mytarget.target + ")", (float)mytarget.actual / (float)mytarget.target);
        else
        {
            EditorUtility.ClearProgressBar();
            buttonPressed = false;
        }
        EditorGUI.EndDisabledGroup();
        EditorUtility.SetDirty(mytarget.gameObject);
    }
}
