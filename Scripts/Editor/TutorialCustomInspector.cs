using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TutorialManager))]
public class TutorialCustomInspector : Editor
{

    public override void OnInspectorGUI()
    {
        TutorialManager tutorial = (TutorialManager)target;
        EditorGUIUtility.labelWidth = 70;
        if (tutorial.sets == null)
        {
            tutorial.sets = new List<TutorialSet>();
        }
        while (tutorial.sets.Count > Enum.GetNames(typeof(TutorialAction)).Length-1)
        {
            tutorial.sets.RemoveAt(tutorial.sets.Count - 1);
        }
        while (tutorial.sets.Count < Enum.GetNames(typeof(TutorialAction)).Length - 1)
        {
            tutorial.sets.Add(new TutorialSet());
        }

        for(int i = 0; i < Enum.GetNames(typeof(TutorialAction)).Length - 1; i++)
        {
            EditorGUILayout.LabelField(((TutorialAction)(i + 1)).ToString(), EditorStyles.boldLabel);
            GUIStyle labelstyle = new GUIStyle();
            labelstyle.fontStyle = FontStyle.Italic;
            EditorGUILayout.LabelField("Controller", labelstyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            tutorial.sets[i].controllerIndication = EditorGUILayout.TextField("Indication", tutorial.sets[i].controllerIndication);
            tutorial.sets[i].controllerAction = EditorGUILayout.TextField("Action", tutorial.sets[i].controllerAction);
            EditorGUILayout.EndVertical();
            tutorial.sets[i].controllerKey = (Sprite)EditorGUILayout.ObjectField("Key", tutorial.sets[i].controllerKey, typeof(Sprite), true);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Keyboard", labelstyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            tutorial.sets[i].keyboardIndication = EditorGUILayout.TextField("Indication", tutorial.sets[i].keyboardIndication);
            tutorial.sets[i].keyboardAction = EditorGUILayout.TextField("Action", tutorial.sets[i].keyboardAction);
            EditorGUILayout.EndVertical();
            tutorial.sets[i].keyboardKey = (Sprite)EditorGUILayout.ObjectField("Key", tutorial.sets[i].keyboardKey, typeof(Sprite), false);
            EditorGUILayout.EndHorizontal();
        }

        EditorUtility.SetDirty(target);

        DrawDefaultInspector();
    }

}
