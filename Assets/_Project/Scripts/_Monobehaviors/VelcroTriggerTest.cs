using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelcroTriggerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnVelcroTriggerEnter(VelcroCollision other)
    {
        Debug.Log(other.gameObject);
    }
}
