using System.Collections.Generic;
using UnityEngine;
using ActionGameEngine.Data;
using Newtonsoft.Json;

public class SpaxJSONSaver : MonoBehaviour
{
    [SerializeField, ReadOnly] private soStateStringHolder _nameHolder;
    [SerializeField] private CharacterData data;

    [HideInInspector] public string characterName;

    //public CharacterData fromSave;
#if UNITY_EDITOR
    public soStateStringHolder NameHolder { get { return this._nameHolder; } }


    public static SpaxJSONSaver instance;

    public void EditorLoadCharacterData(string characterName)
    {
        data = LoadCharacterData(characterName);
        this.characterName = characterName;
        instance = this;


        var so = Resources.Load<soStateStringHolder>("ScriptableObjects/Editor/CharacterData/" + characterName);

        this._nameHolder = so;


        /*int len = data.stateList.Length;
        List<string> nameList = new List<string>();
        for (int i = 0; i < len; i++)
        {
            nameList.Add(data.stateList[i].stateName);
        }

        this._nameHolder.SetStateNames(nameList.ToArray());
    */
    }

    //call to make the states in the state list have their ID's match their index in the list
    //helps prevent states with the same ID
    public void CorrectStateID()
    {
        data.CorrectStateID();
    }

    public string GetStateName(int index)
    {
        return this._nameHolder.GetStateName(index);
    }

    public string GetAnimName(int index)
    {
        return this._nameHolder.GetAnimName(index);
    }
#endif

    public static CharacterData LoadCharacterData(string characterName)
    {
        if (characterName == "")
        {
            Debug.LogError("No character name given, please input a character name");
            return new CharacterData();
        }
        //gets the json file
        TextAsset jsonData = Resources.Load<TextAsset>("JSON/Gameplay/Characters/" + characterName);

        if (jsonData == null)
        {
            Debug.LogError("Could not find character with name: " + characterName + "\n" + "Please input a valid character name to load");
            return new CharacterData();
        }

        //Debug.Log(jsonData);
        return JsonConvert.DeserializeObject<CharacterData>(jsonData.text);
    }

    public void SaveCharacterData(string characterName)
    {

        if (characterName == "")
        {
            Debug.LogError("No character name given, please input a character name");
            return;
        }
        string path = null;
        //if in editor
#if UNITY_EDITOR
        path = "Assets/Resources/JSON/Gameplay/Characters/";
#endif

        //if a built universal windows platform
#if UNITY_WSA
        //Debug.Log("Dadfsfad");
        //path = Application.persistentDataPath +"/Resources/JSON/Gameplay/Characters/";
#endif


        path += characterName + ".json";


        //json = JsonUtility.ToJson(toSave, true);
        //convert the data we want to save into a string to be saved
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        //quick check to make sure path is valid
        //is true if this is an invalid path
        if (Resources.Load<TextAsset>("JSON/Gameplay/Characters/" + characterName) == null)
        {
            Debug.LogError("Could not find character with name: " + characterName + "\n" + "Please input a valid character name to save");
            return;
        }



        //writes the text to the json file
        System.IO.File.WriteAllText(path, json);



        //refresh to correctly see the changes to json
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

}
