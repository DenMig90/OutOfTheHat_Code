using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(ComponentSelector))]
public class IngredientDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var myRect = new Rect(position.x, position.y, 150, position.height);
        
        //SerializedProperty list = property.FindPropertyRelative("allComponents");
        string[] values = new string[ComponentAdder.Instance.allComponents.Count];
        for (int i = 0; i < values.Length; i++)
            values[i] = ComponentAdder.Instance.allComponents[i];

        int index = 0;
        for(int i = 0; i < values.Length; i++)
        {
            if (property.FindPropertyRelative("selected").stringValue == values[i])
                index = i;
        }

        property.FindPropertyRelative("selected").stringValue = values[EditorGUI.Popup(myRect, index, values)];


        EditorGUI.EndProperty();
    }
}