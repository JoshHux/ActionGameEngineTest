using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ActionGameEngine.Enum;

#if UNITY_EDITOR

namespace ActionGameEngine.Data.Drawer
{

    [CustomPropertyDrawer(typeof(StateData))]
    public class StateDataDrawer : PropertyDrawer
    {
        float rectHeight;
        float rectWidth;
        float rectPosY;
        float rectPosX;
        bool hasRChangeExEvent = false;
        bool hasRChangeEnEvent = false;

        soStateStringHolder so = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //scriptable object with name data in it
            if (SpaxJSONSaver.instance == null || SpaxJSONSaver.instance.NameHolder == null)
            {
                return;
            }
            var characterName = SpaxJSONSaver.instance.characterName;

            if (so == null)
            {
                so = SpaxJSONSaver.instance.NameHolder;
            }


            //dimensions for each property rect, height is hardcoded because that prevents certain boxes from extending too far down
            rectHeight = EditorGUIUtility.singleLineHeight;
            rectWidth = position.width;
            rectPosY = position.y;
            rectPosX = position.x;
            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginProperty(position, label, property);

            //STATE NAME PROPERTY
            //find the raw property for the atframe value
            /*var snProp = property.FindPropertyRelative("stateName");
            //rect for the atFrame property
            var snRect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(snProp));
            //label for the atframe value
            var snLabel = new GUIContent("State");
            //draw the field with a +1, fince that make the 0 index frame 1
            var fromGUI = EditorGUI.TextField(snRect, snLabel, snProp.stringValue);
            //log warning if empty state name
            if (fromGUI.Length <= 0) { Debug.LogWarning("Please give a state name for state ID - " + property.FindPropertyRelative("stateID").intValue); }
            //set from gui
            snProp.stringValue = fromGUI;
            //offset the y position properly
            rectPosY += EditorGUI.GetPropertyHeight(snProp, snLabel, true) + EditorGUIUtility.standardVerticalSpacing;

            //find the raw property for the atframe value
            var anProp = property.FindPropertyRelative("animName");
            //rect for the atFrame property
            var anRect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(snProp));
            //label for the atframe value
            var anLabel = new GUIContent("Animation");
            //draw the field with a +1, fince that make the 0 index frame 1
            var anFromGUI = EditorGUI.TextField(anRect, anLabel, anProp.stringValue);
            //log warning if empty state name
            if (anFromGUI.Length <= 0) { Debug.LogWarning("Please give a proper animation to state ID - " + property.FindPropertyRelative("stateID").intValue); }
            //set from gui
            anProp.stringValue = anFromGUI;
            //offset the y position properly
            rectPosY += this.GetYPosOffset(anProp, anLabel);
*/
            int id = property.FindPropertyRelative("stateID").intValue;

            //we're drawing this one manually to do some foldout shenanigans
            //find the raw property for the atframe value
            //var snArrProp = so.FindPropertyRelative("_stateNames");

            //var snProp = snArrProp.GetArrayElementAtIndex(id);
            var sn = so.GetStateName(id);

            //rect for the atFrame property
            var snRect = new Rect(rectPosX + 45f, rectPosY, rectWidth, EditorGUIUtility.singleLineHeight);//EditorGUI.GetPropertyHeight(snProp));
            var snRect2 = new Rect(rectPosX, rectPosY, rectWidth, EditorGUIUtility.singleLineHeight);// EditorGUI.GetPropertyHeight(snProp));
            //label for the atframe value
            var snLabel = new GUIContent("State :");
            //get value from GUI
            var stringFromGUI = EditorGUI.TextField(snRect, sn);
            //set from gui
            so.SetStateName(stringFromGUI, id);
            //offset the y position properly
            rectPosY += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.EndProperty();

            //check if expanded
            property.isExpanded = EditorGUI.Foldout(snRect2, property.isExpanded, snLabel);
            if (property.isExpanded)
            {
                EditorGUI.BeginProperty(position, label, property);

                //ANIMATION NAME PROPERTY
                //var hold = this.DrawPropertyForUs("animName", "Animation", property, true, 1);

                var san = so.GetAnimName(id);

                //rect for the atFrame property
                var sanRect = new Rect(rectPosX + 45f, rectPosY, rectWidth, EditorGUIUtility.singleLineHeight);//EditorGUI.GetPropertyHeight(snProp));
                var sanRect2 = new Rect(rectPosX, rectPosY, rectWidth, EditorGUIUtility.singleLineHeight);// EditorGUI.GetPropertyHeight(snProp));
                                                                                                          //label for the atframe value
                var sanLabel = new GUIContent("State :");
                //get value from GUI
                var astringFromGUI = EditorGUI.TextField(sanRect, san);
                //set from gui
                so.SetAnimName(astringFromGUI, id);
                //draw property
                //EditorGUI.PropertyField(sanRect, prop, label, true);
                //offset the y position properly
                rectPosY += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                //so.SetAnimName(hold.stringValue, id);


                //STATE ID PROPERTY int
                this.DrawPropertyForUs("stateID", "State ID", property, true);

                //PARENT ID PROPERTY int
                this.DrawPropertyForUs("parentID", "Parent ID", property, true);

                //DURATION PROPERTY int
                this.DrawPropertyForUs("duration", "Duration", property, true);

                //STATE CONDITION PROPERTY enum
                this.DrawPropertyForUs("stateConditions", "State Conditions", property, true, 2);

                //CANCEL CONDITION PROPERTY enum
                this.DrawPropertyForUs("cancelConditions", "Cancel Conditions", property, true, 2, 1);

                //ENTER EVENTS PROPERTY enum
                this.DrawPropertyForUs("enterEvents", "Enter Events", property, true, 2, 2);

                //EXIT EVENTS PROPERTY enum
                this.DrawPropertyForUs("exitEvents", "Exit Events", property, true, 2, 2);

                if (hasRChangeEnEvent)
                {
                    //label for the property
                    var resourceLabel = new GUIContent("Change Enter Resources");
                    //find the raw property
                    var resourceProp = property.FindPropertyRelative("enterResources");
                    //rect for the toggleCancelConditions property
                    var resourceRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                    //draw the property with the value
                    EditorGUI.PropertyField(resourceRect, resourceProp, resourceLabel, true);
                    rectPosY += EditorGUI.GetPropertyHeight(resourceProp, resourceLabel, true) + EditorGUIUtility.standardVerticalSpacing;
                }
                if (hasRChangeExEvent)
                {
                    //label for the property
                    var resourceLabel = new GUIContent("Change Exit Resources");
                    //find the raw property
                    var resourceProp = property.FindPropertyRelative("exitResources");
                    //rect for the toggleCancelConditions property
                    var resourceRect = new Rect(rectPosX, rectPosY, rectWidth, rectHeight);
                    //draw the property with the value
                    EditorGUI.PropertyField(resourceRect, resourceProp, resourceLabel, true);
                    rectPosY += EditorGUI.GetPropertyHeight(resourceProp, resourceLabel, true) + EditorGUIUtility.standardVerticalSpacing;
                }


                //TRANSITION PROPERTY array
                var trProp = this.DrawPropertyForUs2("transitions", "Transitions", property);
                //offset the y position properly
                rectPosY += this.GetYPosOffset(trProp, label);

                //FRAMES PROPERTY array
                var frProp = this.DrawPropertyForUs2("frames", "Frames", property);

                EditorGUI.EndProperty();

            }

            EditorUtility.SetDirty(property.serializedObject.targetObject);


        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            //float totalHeight = 0.0f;
            if (property.isExpanded)
            {
                //exclude the first property since that's part of thefoldout label
                //var snProp = property.FindPropertyRelative("stateName");
                var anProp = property.FindPropertyRelative("animName");
                var sidProp = property.FindPropertyRelative("stateID");
                var pidProp = property.FindPropertyRelative("parentID");
                var durProp = property.FindPropertyRelative("duration");
                var scProp = property.FindPropertyRelative("stateConditions");
                var ccProp = property.FindPropertyRelative("cancelConditions");
                var entProp = property.FindPropertyRelative("enterEvents");
                var exiProp = property.FindPropertyRelative("exitEvents");
                var exiRProp = property.FindPropertyRelative("exitResources");
                var entRProp = property.FindPropertyRelative("enterResources");
                var trProp = property.FindPropertyRelative("transitions");
                var frProp = property.FindPropertyRelative("frames");


                hasRChangeEnEvent = EnumHelper.HasEnum((uint)entProp.intValue, (uint)TransitionEvent.DRAIN_RESOURCES);
                hasRChangeExEvent = EnumHelper.HasEnum((uint)exiProp.intValue, (uint)TransitionEvent.DRAIN_RESOURCES);


                //totalHeight += EditorGUI.GetPropertyHeight(snProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(anProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(sidProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(pidProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(durProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(scProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(ccProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(entProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(exiProp) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(trProp, true) + EditorGUIUtility.standardVerticalSpacing;
                totalHeight += (EditorGUI.GetPropertyHeight(exiRProp, true) + EditorGUIUtility.standardVerticalSpacing) * Convert.ToInt32(hasRChangeExEvent);
                totalHeight += (EditorGUI.GetPropertyHeight(entRProp, true) + EditorGUIUtility.standardVerticalSpacing) * Convert.ToInt32(hasRChangeEnEvent);
                totalHeight += EditorGUI.GetPropertyHeight(frProp, true);
            }
            return totalHeight;
        }

        private float GetYPosOffset(SerializedProperty prop, GUIContent label)
        {
            var ret = EditorGUI.GetPropertyHeight(prop, label, true) + EditorGUIUtility.standardVerticalSpacing;
            return ret;
        }

        private SerializedProperty DrawPropertyForUs(string propName, string labelName, SerializedProperty property, bool addHeight, int type = 0, int enumType = 0)
        {
            //find the raw property for the atframe value
            var prop = property.FindPropertyRelative(propName);
            //rect for the atFrame property
            var rect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(prop));
            //label for the atframe value
            var label = new GUIContent(labelName);
            //get value from GUI
            this.AssignValFromGUI(prop, rect, label, type, enumType);

            if (addHeight)
            {
                //offset the y position properly
                rectPosY += this.GetYPosOffset(prop, label);
            }
            return prop;
        }

        //for drawing non-primitive property
        private SerializedProperty DrawPropertyForUs2(string propName, string labelName, SerializedProperty property)
        {
            //find the raw property for the atframe value
            var prop = property.FindPropertyRelative(propName);
            //rect for the atFrame property
            var rect = new Rect(rectPosX, rectPosY, rectWidth, EditorGUI.GetPropertyHeight(prop));
            //label for the atframe value
            var label = new GUIContent(labelName);
            //draw property
            EditorGUI.PropertyField(rect, prop, label, true);

            return prop;
        }

        private void AssignValFromGUI(SerializedProperty property, Rect rect, GUIContent label, int type = 0, int enumType = 0)
        {

            //change data value based on int
            switch (type)
            {
                //int
                case 0:
                    //draw the field with a +1, fince that make the 0 index frame 1
                    var intFromGUI = EditorGUI.IntField(rect, label, property.intValue);
                    //set from gui
                    property.intValue = intFromGUI;
                    break;
                //string
                case 1:
                    //draw the field with a +1, fince that make the 0 index frame 1
                    var stringFromGUI = EditorGUI.TextField(rect, label, property.stringValue);
                    //set from gui
                    //property.stringValue = stringFromGUI;
                    break;
                //enum
                case 2:
                    int enumFromGUI = 0;
                    //change enum value based on int
                    switch (enumType)
                    {
                        //StateCondition
                        case 0:
                            //draw the field with a +1, fince that make the 0 index frame 1
                            enumFromGUI = (int)((StateCondition)EditorGUI.EnumFlagsField(rect, label, (StateCondition)property.intValue));

                            break;
                        //CancelCondition
                        case 1:
                            //draw the field with a +1, fince that make the 0 index frame 1
                            enumFromGUI = (int)((CancelConditions)EditorGUI.EnumFlagsField(rect, label, (CancelConditions)property.intValue));

                            break;
                        //TransitionEvent
                        case 2:
                            //draw the field with a +1, fince that make the 0 index frame 1
                            enumFromGUI = (int)((TransitionEvent)EditorGUI.EnumFlagsField(rect, label, (TransitionEvent)property.intValue));

                            break;
                    }
                    //set from gui
                    property.intValue = enumFromGUI;
                    break;
                default:
                    Debug.LogError("data type not found for " + label.text);
                    break;
            }
        }
    }
}
#endif