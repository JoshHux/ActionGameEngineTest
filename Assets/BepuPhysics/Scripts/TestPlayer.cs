using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BEPUUnity;
using FixMath.NET;
public class TestPlayer : MonoBehaviour
{
    public ShapeBase rb;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.GetEntity().linearVelocity = new BEPUutilities.BepuVector3((Fix64)Input.GetAxis("Horizontal"), 0, (Fix64)Input.GetAxis("Vertical"));
        rb.GetEntity().angularVelocity = new BEPUutilities.BepuVector3((Fix64)Input.GetAxis("Horizontal"), 0, (Fix64)Input.GetAxis("Vertical"));

    }
}
