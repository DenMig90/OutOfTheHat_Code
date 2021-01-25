using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraTriggerBehaviour))]
public class CameraTriggerCustomInspector : Editor
{

    public override void OnInspectorGUI()
    {
        CameraTriggerBehaviour ct = (CameraTriggerBehaviour)target;

        //    changeOffsetX;
        //public bool changeOffsetY;
        //public bool changeDistance;
        //public bool changeOffsetSpeed;
        //public bool changeDistanceSpeed;
        //public bool changeOffsetCurve;
        //public bool changeDistanceCurve;

        GUILayout.BeginHorizontal();
        ct.changeOffsetX = EditorGUILayout.Toggle("ChangeOffsetX:", ct.changeOffsetX);
        if(ct.changeOffsetX)
            ct.offsetX = EditorGUILayout.FloatField(ct.offsetX);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ct.changeOffsetY = EditorGUILayout.Toggle("ChangeOffsetY:", ct.changeOffsetY);
        if (ct.changeOffsetY)
            ct.offsetY = EditorGUILayout.FloatField(ct.offsetY);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ct.changeDistance = EditorGUILayout.Toggle("ChangeDistance:", ct.changeDistance);
        if (ct.changeDistance)
            ct.distance = EditorGUILayout.FloatField(ct.distance);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ct.changeOffsetSpeed = EditorGUILayout.Toggle("ChangeOffsetSpeed:", ct.changeOffsetSpeed);
        if (ct.changeOffsetSpeed)
            ct.offsetSpeed = EditorGUILayout.FloatField(ct.offsetSpeed);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ct.changeDistanceSpeed = EditorGUILayout.Toggle("ChangeDistanceSpeed:", ct.changeDistanceSpeed);
        if (ct.changeDistanceSpeed)
            ct.distanceSpeed = EditorGUILayout.FloatField(ct.distanceSpeed);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ct.changeVerticalDeadzone = EditorGUILayout.Toggle("ChangeVerticalDeadzone:", ct.changeVerticalDeadzone);
        if (ct.changeVerticalDeadzone)
            ct.verticalDeadZone = (float)Math.Round(EditorGUILayout.Slider(ct.verticalDeadZone, 0, 1), 2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        ct.blockVerticalDeadzone = EditorGUILayout.Toggle("BlockVerticalDeadzone:", ct.blockVerticalDeadzone);
        GUILayout.EndHorizontal();

        if (!ct.changeOffsetX)
        {
            GUILayout.BeginHorizontal();
            ct.maintainPlayerXPosition = EditorGUILayout.Toggle("MaintainPlayerXPosition:", ct.maintainPlayerXPosition);
            GUILayout.EndHorizontal();
        }
        else
        {
            ct.maintainPlayerXPosition = false;
        }

        if (!ct.changeOffsetY)
        {
            GUILayout.BeginHorizontal();
            ct.maintainPlayerYPosition = EditorGUILayout.Toggle("MaintainPlayerYPosition:", ct.maintainPlayerYPosition);
            GUILayout.EndHorizontal();
        }
        else
        {
            ct.maintainPlayerYPosition = false;
        }

        if (ct.changeOffsetX || ct.changeOffsetY)
        {
            GUILayout.BeginHorizontal();
            ct.changeOffsetCurve = EditorGUILayout.Toggle("ChangeOffsetCurve:", ct.changeOffsetCurve);
            if (ct.changeOffsetCurve)
                ct.offsetCurve = EditorGUILayout.CurveField(ct.offsetCurve);
            GUILayout.EndHorizontal();
        }

        if (ct.changeDistance)
        {
            GUILayout.BeginHorizontal();
            ct.changeDistanceCurve = EditorGUILayout.Toggle("ChangeDistanceCurve:", ct.changeDistanceCurve);
            if (ct.changeDistanceCurve)
                ct.distanceCurve = EditorGUILayout.CurveField(ct.distanceCurve);
            GUILayout.EndHorizontal();
        }

        EditorUtility.SetDirty(target);
    }

}
