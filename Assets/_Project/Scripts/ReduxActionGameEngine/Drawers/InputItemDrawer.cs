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
            var fromGUI = (DigitalInput)EditorGUI.EnumPopup(position, label, InputItem.ToDigInp((short)rawProp.intValue));

            rawProp.intValue = InputItem.DigToRaw(fromGUI);

            EditorGUI.EndProperty();
            /*position.y += position.height * 2;
            EditorGUI.BeginProperty(position, new GUIContent("thing"), property);

            var rawProp2 = property.FindPropertyRelative("m_rawValue");
            var fromGUI2 = EditorGUI.IntField(position, new GUIContent("thing"), rawProp2.intValue);

            //rawProp.intValue = InputItem.DigToRaw(fromGUI2);

            EditorGUI.EndProperty();
*/
        }
    }
}
