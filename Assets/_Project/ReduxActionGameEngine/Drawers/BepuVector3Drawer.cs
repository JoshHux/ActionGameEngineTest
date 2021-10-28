using UnityEditor;
using UnityEngine;

namespace BEPUutilities.Drawers
{

    [CustomPropertyDrawer(typeof(BepuVector3))]
    public class BepuVector3Drawer : PropertyDrawer
    {

        private const int INDENT_OFFSET = 15;
        private const int LABEL_WIDTH = 12;
        private const int LABEL_MARGIN = 1;

        private static GUIContent xLabel = new GUIContent("X");
        private static GUIContent yLabel = new GUIContent("Y");
        private static GUIContent zLabel = new GUIContent("Z");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            position.width /= 3f;
            float indentOffsetLevel = (INDENT_OFFSET) * EditorGUI.indentLevel;
            position.width += indentOffsetLevel;

            EditorGUIUtility.labelWidth = indentOffsetLevel + LABEL_WIDTH;

            SerializedProperty xSerProperty = property.FindPropertyRelative("X");
            position.x -= indentOffsetLevel;
            EditorGUI.PropertyField(position, xSerProperty, xLabel);

            position.x += position.width;

            SerializedProperty ySerProperty = property.FindPropertyRelative("Y");
            position.x -= indentOffsetLevel;
            EditorGUI.PropertyField(position, ySerProperty, yLabel);

            position.x += position.width;

            SerializedProperty zSerProperty = property.FindPropertyRelative("Z");
            position.x -= indentOffsetLevel;
            EditorGUI.PropertyField(position, zSerProperty, zLabel);

            EditorGUI.EndProperty();
        }

    }
}