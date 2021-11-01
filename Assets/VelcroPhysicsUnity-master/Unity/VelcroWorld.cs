using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VelcroPhysics.Dynamics;

public class VelcroWorld : MonoBehaviour
{
    public static VelcroWorld instance;

    private VelcroWorldManager2D manager;

    // Start is called before the first frame update
    void Awake()
    {

        manager.Initialize();

        instance = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        manager.Step();
    }

    public void AddBody(VelcroBody newBody)
    {
        manager.AddBody(newBody);
    }


    public void RemoveBody(VelcroBody newBody)
    {
        manager.RemoveBody(newBody);
    }

    public World GetWorld() { return manager.GetWorld(); }
}
