using UnityEditor;
using UnityEngine;
using ActionGameEngine.Enum;
#if UNITY_EDITOR
namespace ActionGameEngine.Data.Drawer
{
    [CustomPropertyDrawer(typeof(CommandList))]

    public class CommandListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);


            //label for the property
            var hitboxLabel = new GUIContent("command");
            //find the raw property
            var hitboxProp = property.FindPropertyRelative("command");
            //rect for the toggleCancelConditions property
            var hitboxRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(hitboxProp, true) + EditorGUIUtility.standardVerticalSpacing);
            //draw the property with the value
            position.height = EditorGUI.GetPropertyHeight(hitboxProp, true) + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, hitboxProp, hitboxLabel, true);

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;// + EditorGUIUtility.standardVerticalSpacing;
            //float totalHeight = 0.0f;

            //var frameProp = property.FindPropertyRelative("atFrame");
            var flagProp = property.FindPropertyRelative("command");

            //totalHeight += EditorGUI.GetPropertyHeight(frameProp) + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUI.GetPropertyHeight(flagProp) + EditorGUIUtility.standardVerticalSpacing;
            //Debug.Log("fad");

            return totalHeight;
        }
    }
}
#endif
