using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VelcroPhysics.Dynamics;
using FixMath.NET;

public class VelcroWorld : MonoBehaviour
{
    public static VelcroWorld instance;

    private VelcroWorldManager2D manager;
    private int[] collisionMatrix;

    // Start is called before the first frame update
    void Awake()
    {
        manager = new VelcroWorldManager2D();
        manager.Initialize();

        collisionMatrix = new int[32];
        int len = 32;
        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < len; j++)
            {
                bool collides = Physics2D.GetIgnoreLayerCollision(i, j);

                if (collides)
                {
                    collisionMatrix[i] |= 1 << j;
                }
            }
        }
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
    public int GetCollisions(int layer) { return collisionMatrix[layer]; }
}
