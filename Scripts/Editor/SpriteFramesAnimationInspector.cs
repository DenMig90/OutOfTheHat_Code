using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteFramesAnimation))]
public class SpriteFramesAnimationInspector : Editor
{
    private bool buttonPressed;
    public override void OnInspectorGUI()
    {
        SpriteFramesAnimation mytarget = (SpriteFramesAnimation)target;
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Load From Folder", style))
        {
            string path = EditorUtility.OpenFolderPanel("Select Frames Folder", "Assets/Resources","");
            //Debug.Log(location);
            string toremove = Application.dataPath + "/Resources/";
            //Debug.Log(toremove);
            path = path.Replace(toremove, "");
            //Debug.Log(location);
            mytarget.Load(path);
            buttonPressed = false;
            EditorUtility.SetDirty(mytarget.gameObject);
        }
        DrawDefaultInspector();
    }
}

