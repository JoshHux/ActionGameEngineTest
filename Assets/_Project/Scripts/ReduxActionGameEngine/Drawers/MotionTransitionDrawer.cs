using UnityEditor;
using UnityEngine;
using ActionGameEngine.Data;
#if UNITY_EDITOR
namespace ActionGameEngine.Data.Drawer
{
    [CustomPropertyDrawer(typeof(MotionTransition))]

    public class MotionTransitionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (SpaxJSONSaver.instance == null) { return; }

            //position.height = 16;
            //position.height = 16;
            //dimensions for each property rect, height is hardcoded because that prevents certain boxes from extending too far down
            var rectHeight = EditorGUIUtility.singleLineHeight;
            var rectWidth = position.width;
            var rectPosY = position.y;
            var rectPosX = position.x;
            // draw label, it returns modified rect
            //position = EditorGUI.PrefixLabel(position, new GUIContent("Stuff:"));
            //position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;

            //label for the atframe value
            var frameLabel = new GUIContent("At Frame :");



            var characterName = SpaxJSONSaver.instance.characterName;
            var so = Resources.Load<soStateStringHolder>("ScriptableObjects/Editor/CharacterData/" + characterName);

            var moveTarget = so.GetStateName(property.FindPropertyRelative("targetState").intValue);

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, moveTarget);
            if (property.isExpanded)
            {
                EditorGUI.BeginProperty(position, label, property);

                rectPosY += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                //label for the property
                var hitboxLabel = new GUIContent("targetState");
                //find the raw property
                var hitboxProp = property.FindPropertyRelative("targetState");
                //rect for the toggleCancelConditions property
                var hitboxRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                EditorGUI.PropertyField(hitboxRect, hitboxProp, hitboxLabel, true);

                rectPosY += EditorGUI.GetPropertyHeight(hitboxProp, hitboxLabel, true) + EditorGUIUtility.standardVerticalSpacing;

                //label for the property
                var crLabel = new GUIContent("cancelRequirements");
                //find the raw property
                var crProp = property.FindPropertyRelative("cancelRequirements");
                //rect for the toggleCancelConditions property
                var crRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                EditorGUI.PropertyField(crRect, crProp, crLabel, true);

                rectPosY += EditorGUI.GetPropertyHeight(crProp, crLabel, true) + EditorGUIUtility.standardVerticalSpacing;

                //label for the property
                var cmdLabel = new GUIContent("cmdMotion");
                //find the raw property
                var cmdProp = property.FindPropertyRelative("cmdMotion");
                //rect for the toggleCancelConditions property
                var cmdRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                EditorGUI.PropertyField(cmdRect, cmdProp, cmdLabel, true);

                rectPosY += EditorGUI.GetPropertyHeight(cmdProp, cmdLabel, true) + EditorGUIUtility.standardVerticalSpacing;

                //label for the property
                var rsrcLabel = new GUIContent("resources");
                //find the raw property
                var rsrcProp = property.FindPropertyRelative("resources");
                //rect for the toggleCancelConditions property
                var rsrcRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                EditorGUI.PropertyField(rsrcRect, rsrcProp, rsrcLabel, true);

                rectPosY += EditorGUI.GetPropertyHeight(rsrcProp, rsrcLabel, true) + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.EndProperty();
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            //float totalHeight = 0.0f;
            if (property.isExpanded)
            {
                //var frameProp = property.FindPropertyRelative("atFrame");
                var targetProp = property.FindPropertyRelative("targetState");
                var crProp = property.FindPropertyRelative("cancelRequirements");
                var cmdProp = property.FindPropertyRelative("cmdMotion");
                var rsrcProp = property.FindPropertyRelative("resources");

                //totalHeight += EditorGUI.GetPropertyHeight(frameProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(targetProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(crProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(cmdProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(rsrcProp) + EditorGUIUtility.standardVerticalSpacing;
            }
            return totalHeight;
        }

    }
}
#endif
