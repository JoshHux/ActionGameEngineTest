using System;
using UnityEditor;
using UnityEngine;
using ActionGameEngine.Enum;

namespace ActionGameEngine.Input.Drawer
{

    [CustomPropertyDrawer(typeof(InputItem))]

    public class InputItemDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var rawProp = property.FindPropertyRelative("m_rawValue");
            EditorGUI.IntField(position, label, rawProp.intValue);


            EditorGUI.EndProperty();

        }
    }
}
