using UnityEngine;
using ActionGameEngine.Data;
using Newtonsoft.Json;

public class SpaxJSONSaver : MonoBehaviour
{
    [SerializeField]
    private CharacterData data;

    //public CharacterData fromSave;
#if UNITY_EDITOR
    public void EditorLoadCharacterData(string characterName)
    {
        data = LoadCharacterData(characterName);
    }

    //call to make the states in the state list have their ID's match their index in the list
    //helps prevent states with the same ID
    public void CorrectStateID()
    {
        data.CorrectStateID();
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
        path = Application.persistentDataPath +"/Resources/JSON/Gameplay/Characters/";
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
