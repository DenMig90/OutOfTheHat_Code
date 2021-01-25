using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
public class LayerSelectorPropertyDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        //var myRect = new Rect(position.x, position.y, position.width, position.height);
        property.intValue = EditorGUI.LayerField(position, property.intValue, EditorStyles.popup);
        EditorGUI.EndProperty();
    }
}
