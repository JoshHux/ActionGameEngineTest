using System.Collections;
using System.Collections.Generic;
using Spax;
using Spax.StateMachine;
using UnityEditor;
using UnityEngine;
using ActionGameEngine.Input;

public class SpaxJSONSaver : MonoBehaviour
{
    public CharacterData toSave;

    public CharacterData fromSave;

    public string json;
    public LinkedList<RecorderElement> prevInputs;

    public RecorderElement[] lst;
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

        //refresh to correctly see the changes to json
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif


        //System.IO.File.WriteAllText(Application.persistentDataPath + "/Assets/Resources/JSON/TestData/Test.json", json);
        System.IO.File.WriteAllText(path, json);

        TextAsset jsonData =
            Resources.Load<TextAsset>("JSON/TestData/Test");
        Debug.Log(jsonData);
        fromSave = JsonUtility.FromJson<CharacterData>(jsonData.text);

        prevInputs = new LinkedList<RecorderElement>();
        prevInputs.AddFirst(new RecorderElement());

        RecorderElement first = prevInputs.First.Value;
        first.framesHeld++;


        prevInputs.RemoveFirst();
        prevInputs.AddFirst(first);

        lst = new RecorderElement[10];
        prevInputs.CopyTo(lst, 0);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
