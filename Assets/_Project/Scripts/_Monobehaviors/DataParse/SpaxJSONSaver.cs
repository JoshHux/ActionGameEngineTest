using System.Collections.Generic;
using FixMath.NET;
using UnityEngine;
using ActionGameEngine.Input;
using ActionGameEngine.Data;
using Newtonsoft.Json;

public class SpaxJSONSaver : MonoBehaviour
{
    public Fix64 test;
    [SerializeField]
    private CharacterData toSave;

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


        //json = JsonUtility.ToJson(toSave, true);
        json = JsonConvert.SerializeObject(toSave, Formatting.Indented);


        //System.IO.File.WriteAllText(Application.persistentDataPath + "/Assets/Resources/JSON/TestData/Test.json", json);
        System.IO.File.WriteAllText(path, json);
        //refresh to correctly see the changes to json
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        fromSave = LoadDataFromPath(path);

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

    public static CharacterData LoadDataFromPath(string path)
    {


        //gets the json file
        TextAsset jsonData = Resources.Load<TextAsset>("JSON/TestData/Test");
        //Debug.Log(jsonData);
        return JsonConvert.DeserializeObject<CharacterData>(jsonData.text);
    }
}
