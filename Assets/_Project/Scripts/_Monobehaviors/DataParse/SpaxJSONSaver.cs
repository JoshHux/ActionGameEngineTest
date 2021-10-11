using System.Collections;
using System.Collections.Generic;
using Spax;
using Spax.StateMachine;
using UnityEditor;
using UnityEngine;

public class SpaxJSONSaver : MonoBehaviour
{
    public CharacterData toSave;

    public CharacterData fromSave;

    public string json;

    // Start is called before the first frame update
    void Start()
    {
        string path = null;

        //if in editor
#if UNITY_EDITOR
        path = "Assets/Resources/JSON/TestData/Test.json";
#endif


        //if a built universal windows platform
#if UNITY_WSA
        //Debug.Log("Dadfsfad");
        path =
            Application.persistentDataPath +
            "/Resources/JSON/TestData/Test.json";
#endif


        json = JsonUtility.ToJson(toSave, true);

        //System.IO.File.WriteAllText(Application.persistentDataPath + "/Assets/Resources/JSON/TestData/Test.json", json);
        System.IO.File.WriteAllText (path, json);

        fromSave = JsonUtility.FromJson<CharacterData>(json);

        //refresh to correctly see the changes to json
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // Update is called once per frame
    void Update()
    {
    }
}
