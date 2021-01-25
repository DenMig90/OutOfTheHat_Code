using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExtrudeSprite))]
public class ExtrudeSpriteCustomInspector : Editor
{
    private float oldZ = 0f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ExtrudeSprite mytarget = (ExtrudeSprite)target;
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        if (mytarget.working)
        {

            Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            EditorGUI.ProgressBar(rect, mytarget.progress, "Doing stuff!");
        }
        else
        {
            if (mytarget.GetComponent<PolygonCollider2D>() != null)
            {
                if (GUILayout.Button("Extrude", style))
                {
                    mytarget.StartCoroutine("Generate");
                }
            }
            if (mytarget.GetComponent<MeshCollider>() != null)
            {
                if (GUILayout.Button("Return to 2D", style))
                {
                    mytarget.StartCoroutine("DeGenerate");
                }
            }
        }
        EditorUtility.SetDirty(mytarget.gameObject);

    }
}