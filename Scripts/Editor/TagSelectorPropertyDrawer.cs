using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        //var myRect = new Rect(position.x, position.y, position.width, position.height);
        property.stringValue = EditorGUI.TagField(position, property.stringValue, EditorStyles.popup);
        EditorGUI.EndProperty();
    }
}