using System;
using UnityEngine;
using UnityEditor;
using ActionGameEngine.Data;
using ActionGameEngine.Enum;
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FrameData))]
public class FrameDrawer : PropertyDrawer
{
    private bool hasTimerEvent = false;
    private bool hasVelocityEvent = false;
    private bool hasHitboxes = false;
    private bool hasHurtboxes = false;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
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

        //show = EditorGUI.Foldout(position, property.isExpanded, label);
        //show = property.isExpanded;
        EditorGUI.BeginProperty(position, label, property);
        //find the raw property for the atframe value
        var frameProp = property.FindPropertyRelative("atFrame");
        //rect for the atFrame property
        var frameRect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(frameProp));
        //label for the atframe value
        var frameLabel = new GUIContent("At Frame");
        //draw the field with a +1, fince that make the 0 index frame 1
        var fromGUI = EditorGUI.IntField(frameRect, frameLabel, frameProp.intValue + 1);
        //check for invalid atFrame value, log error if found
        if (fromGUI <= 0) { Debug.LogError("Invalid atFrame value, events cannot occur on frame 0"); }
        //-1 to get the frame back to index
        frameProp.intValue = fromGUI - 1;
        //offset the y position properly
        rectPosY += EditorGUI.GetPropertyHeight(frameProp, frameLabel, true) + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.EndProperty();


        //EditorGUI.BeginFoldoutHeaderGroup(position, show, label);
        //property.isExpanded = show;
        property.isExpanded = EditorGUI.Foldout(frameRect, property.isExpanded, frameLabel);
        //if (show)
        if (property.isExpanded)
        {
            // reset indentation
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            //rectPosY += EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginProperty(position, label, property);
            /* //find the raw property for the atframe value
             var frameProp = property.FindPropertyRelative("atFrame");
             //rect for the atFrame property
             var frameRect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(frameProp));
             //label for the atframe value
             var frameLabel = new GUIContent("At Frame");
             //draw the field with a +1, fince that make the 0 index frame 1
             var fromGUI = EditorGUI.IntField(frameRect, frameLabel, frameProp.intValue + 1);
             //check for invalid atFrame value, log error if found
             if (fromGUI <= 0) { Debug.LogError("Invalid atFrame value, events cannot occur on frame 0"); }
             //-1 to get the frame back to index
             frameProp.intValue = fromGUI - 1;
             //offset the y position properly
             rectPosY += EditorGUI.GetPropertyHeight(frameProp, frameLabel, true) + EditorGUIUtility.standardVerticalSpacing;
             //EditorGUI.EndProperty();
 */

            //find the raw property
            var flagProp = property.FindPropertyRelative("flags");
            //rect for the toggleStateConditions property
            var flagRect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(frameProp));
            //label for the property
            var flagLabel = new GUIContent("Event Flags");
            //draw the property with the value
            var fromFlags = (FrameEventFlag)EditorGUI.EnumFlagsField(flagRect, flagLabel, (FrameEventFlag)flagProp.intValue);
            //reassign value from editor
            flagProp.intValue = (int)fromFlags;
            //offset the y position properly
            rectPosY += EditorGUI.GetPropertyHeight(flagProp, flagLabel, true) + EditorGUIUtility.standardVerticalSpacing;

            //setting bool values based on what even enums we have
            hasTimerEvent = EnumHelper.HasEnum((uint)flagProp.intValue, (uint)FrameEventFlag.SET_TIMER);
            hasVelocityEvent = EnumHelper.HasEnum((uint)flagProp.intValue, (uint)FrameEventFlag.APPLY_VEL);
            hasHitboxes = EnumHelper.HasEnum((uint)flagProp.intValue, (uint)FrameEventFlag.ACTIVATE_HITBOXES);
            hasHurtboxes = EnumHelper.HasEnum((uint)flagProp.intValue, (uint)FrameEventFlag.ACTIVATE_HURTBOXES);



            //find the raw property
            var sttCondProp = property.FindPropertyRelative("toggleStateConditions");
            //rect for the toggleStateConditions property
            var sttCondRect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(frameProp));
            //label for the property
            var sttCondLabel = new GUIContent("State Conditions");
            //draw the property with the value
            var fromGUISttCond = (StateCondition)EditorGUI.EnumFlagsField(sttCondRect, sttCondLabel, (StateCondition)sttCondProp.intValue);
            //reassign value from editor
            sttCondProp.intValue = (int)fromGUISttCond;
            //offset the y position properly
            rectPosY += EditorGUI.GetPropertyHeight(sttCondProp, sttCondLabel, true) + EditorGUIUtility.standardVerticalSpacing;

            //find the raw property
            var canCondProp = property.FindPropertyRelative("toggleCancelConditions");
            //rect for the toggleCancelConditions property
            var canCondRect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(frameProp));
            //label for the property
            var canCondLabel = new GUIContent("Cancel Conditions");
            //draw the property with the value
            var fromGUICanCond = (CancelConditions)EditorGUI.EnumFlagsField(canCondRect, canCondLabel, (CancelConditions)canCondProp.intValue);
            //reassign value from editor
            canCondProp.intValue = (int)fromGUICanCond;
            //offset the y position properly
            rectPosY += EditorGUI.GetPropertyHeight(canCondProp, canCondLabel, true) + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.EndProperty();

            if (hasVelocityEvent)
            {
                //label for the property
                var velLabel = new GUIContent("Applied Velocity");
                //find the raw property
                var velProp = property.FindPropertyRelative("frameVelocity");
                //rect for the toggleCancelConditions property
                var velRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                //draw the property with the value
                EditorGUI.PropertyField(velRect, velProp, velLabel, true);
                //offset the y position properly
                rectPosY += EditorGUI.GetPropertyHeight(velProp, velLabel, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            if (hasTimerEvent)
            {
                //label for the property
                var timerLabel = new GUIContent("Timer Event Data");
                //find the raw property
                var timerProp = property.FindPropertyRelative("timerEvent");
                //rect for the toggleCancelConditions property
                var timerRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                //draw the property with the value
                EditorGUI.PropertyField(timerRect, timerProp, timerLabel, true);
                rectPosY += EditorGUI.GetPropertyHeight(timerProp, timerLabel, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            if (hasHitboxes)
            {
                //label for the property
                var hitboxLabel = new GUIContent("Hitboxes");
                //find the raw property
                var hitboxProp = property.FindPropertyRelative("hitboxes");
                //rect for the toggleCancelConditions property
                var hitboxRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                //draw the property with the value
                EditorGUI.PropertyField(hitboxRect, hitboxProp, hitboxLabel, true);
                rectPosY += EditorGUI.GetPropertyHeight(hitboxProp, hitboxLabel, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            if (hasHurtboxes)
            {
                //label for the property
                var hurtboxLabel = new GUIContent("Hurtboxes");
                //find the raw property
                var hurtboxProp = property.FindPropertyRelative("hurtboxes");
                //rect for the toggleCancelConditions property
                var hurtboxRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                //draw the property with the value
                EditorGUI.PropertyField(hurtboxRect, hurtboxProp, hurtboxLabel, true);
                //rectPosY += EditorGUI.GetPropertyHeight(hurtboxProp, hurtboxLabel, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            // indent back to where it was
            EditorGUI.indentLevel = indent;
        }
        EditorUtility.SetDirty(property.serializedObject.targetObject);


    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        //float totalHeight = 0.0f;
        if (property.isExpanded)
        {
            //var frameProp = property.FindPropertyRelative("atFrame");
            var flagProp = property.FindPropertyRelative("flags");
            var timerProp = property.FindPropertyRelative("timerEvent");
            var velProp = property.FindPropertyRelative("frameVelocity");
            var sttCondProp = property.FindPropertyRelative("toggleStateConditions");
            var canCondProp = property.FindPropertyRelative("toggleCancelConditions");
            var hitboxProp = property.FindPropertyRelative("hitboxes");
            var hurtboxProp = property.FindPropertyRelative("hurtboxes");


            hasTimerEvent = EnumHelper.HasEnum((uint)flagProp.intValue, (uint)FrameEventFlag.SET_TIMER);
            hasVelocityEvent = EnumHelper.HasEnum((uint)flagProp.intValue, (uint)FrameEventFlag.APPLY_VEL);
            hasHitboxes = EnumHelper.HasEnum((uint)flagProp.intValue, (uint)FrameEventFlag.ACTIVATE_HITBOXES);
            hasHurtboxes = EnumHelper.HasEnum((uint)flagProp.intValue, (uint)FrameEventFlag.ACTIVATE_HURTBOXES);

            //totalHeight += EditorGUI.GetPropertyHeight(frameProp) + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUI.GetPropertyHeight(flagProp) + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += EditorGUI.GetPropertyHeight(sttCondProp) + EditorGUIUtility.standardVerticalSpacing;
            totalHeight += (EditorGUI.GetPropertyHeight(timerProp, true) + EditorGUIUtility.standardVerticalSpacing) * Convert.ToInt32(hasTimerEvent);
            totalHeight += (EditorGUI.GetPropertyHeight(velProp, true) + EditorGUIUtility.standardVerticalSpacing) * Convert.ToInt32(hasVelocityEvent);
            totalHeight += (EditorGUI.GetPropertyHeight(hitboxProp, true) + EditorGUIUtility.standardVerticalSpacing) * Convert.ToInt32(hasHitboxes);
            totalHeight += (EditorGUI.GetPropertyHeight(hurtboxProp, true) + EditorGUIUtility.standardVerticalSpacing) * Convert.ToInt32(hasHurtboxes);
            totalHeight += EditorGUI.GetPropertyHeight(canCondProp);
        }
        return totalHeight;
    }

}
#endif
