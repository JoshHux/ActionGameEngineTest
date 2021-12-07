using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpaxJSONSaver))]
public class JSONSaverEditor : Editor
{
    private string characterName = "Test";
    public override void OnInspectorGUI()
    {
        SpaxJSONSaver saver = (SpaxJSONSaver)target;
        EditorGUILayout.HelpBox(("This is a an editor script to make changing character data easier\n" +
        "\"Save Data\" will save the data to the character, \"Load Data\" will bring up a character's data\n"
        + "Always remember to save you're work!"), MessageType.Info);

        EditorGUILayout.Space(10);
        characterName = EditorGUILayout.TextField("Character Name", characterName);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Data"))
        {
            saver.EditorLoadCharacterData(characterName);
        }
        if (GUILayout.Button("Save Data"))
        {
            saver.SaveCharacterData(characterName);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Auto-Assign State Id's"))
        {
            saver.CorrectStateID();
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector();


    }
}
