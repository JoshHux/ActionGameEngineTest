using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spax.StateMachine;
public class SpaxJSONSaver : MonoBehaviour
{
    public HitBoxData toSave;
    public string json;
    // Start is called before the first frame update
    void Start()
    {
        json = JsonUtility.ToJson(toSave);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/Resources/JSON/TestData/Test.json", json);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
