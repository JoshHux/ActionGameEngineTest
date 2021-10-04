using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spax.StateMachine;
public class SpaxJSONSaver : MonoBehaviour
{
    public CharacterFrame toSave;
    public CharacterFrame fromSave;

    public string json;
    // Start is called before the first frame update
    void Start()
    {
        json = JsonUtility.ToJson(toSave, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/Assets/Resources/JSON/TestData/Test.json", json);

        fromSave = JsonUtility.FromJson<CharacterFrame>(json);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
