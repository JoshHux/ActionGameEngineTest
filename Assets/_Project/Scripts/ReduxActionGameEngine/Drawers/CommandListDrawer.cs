using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ActionGameEngine.Data;
namespace ActionGameEngine.Input.Drawer
{
    /*   [CustomPropertyDrawer(typeof(CommandList))]

       public class CommandListDrawer : Editor
       {

           public override void OnInspectorGUI()
           {
               EditorGUILayout.LabelField("Custom editor:");
               var serializedObject = new SerializedObject(target);
               var property = serializedObject.FindProperty("command");
               serializedObject.Update();
               EditorGUILayout.PropertyField(property, true);
               serializedObject.ApplyModifiedProperties();
           }

       }
       */
}