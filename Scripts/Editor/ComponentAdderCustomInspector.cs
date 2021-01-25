using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ComponentAdder))]
public class ComponentAdderCustomInspector : Editor {
    public void OnEnable()
    {
        ComponentAdder mytarget = (ComponentAdder)target;
        mytarget.GetAllComponents();
        EditorUtility.SetDirty(mytarget.gameObject);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ComponentAdder mytarget = (ComponentAdder)target;
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;
        style.fontStyle = FontStyle.Normal;
        if (GUILayout.Button("Refresh List", style))
        {
            mytarget.GetAllComponents();
        }
        if (GUILayout.Button("Add Component", style))
        {
            mytarget.AddComponent();
        }
        EditorUtility.SetDirty(mytarget.gameObject);
    }
}
