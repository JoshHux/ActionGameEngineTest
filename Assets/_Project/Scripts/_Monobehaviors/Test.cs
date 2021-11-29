using VelcroPhysics.Unity;
using UnityEngine;
using FixMath.NET;
public class Test : MonoBehaviour
{
    public bool setPos;
    VelcroBody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<VelcroBody>();


    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (setPos)
        {
            rb.Position = rb.Position + new FVector2(0, -(Fix64)Time.fixedDeltaTime * 3);
        }
        else
        {
            rb.Velocity = new FVector2(0, -3);

        }
    }
}
