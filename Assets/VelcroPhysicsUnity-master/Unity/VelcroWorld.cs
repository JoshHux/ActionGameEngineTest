using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Collision.Filtering;
using FixMath.NET;

public class VelcroWorld : MonoBehaviour
{
    public static VelcroWorld instance;

    private VelcroWorldManager2D manager;
    [SerializeField] private Category[] collisionMatrix;

    // Start is called before the first frame update
    void Awake()
    {

        //get the collision matrix and set the corrext filters accordingly
        collisionMatrix = new Category[32];
        int len = 10;
        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < len; j++)
            {
                bool collides = !Physics.GetIgnoreLayerCollision(i, j);

                if (collides)
                {
                    collisionMatrix[i] |= (Category)(1 << j);
                }
            }
        }
        instance = this;
        manager = new VelcroWorldManager2D();
        manager.Initialize();
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
    public Category GetCollisions(int layer) { return collisionMatrix[layer]; }
}
