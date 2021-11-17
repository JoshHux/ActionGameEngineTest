using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor.Animations;
using FixedAnimationSystem;
using FixMath.NET;
public class AnimationReader : MonoBehaviour
{
    public AnimatorController controller;

    [SerializeField] private Vector3[] allPositions;
    [SerializeField] private Vector3[] allScales;
    [SerializeField] private Quaternion[] allRotations;

    [SerializeField] private FixedAnimation[] fixedAnimations;

    public bool play = false;


    // Start is called before the first frame update
    void Start()
    {
        //to get animations directly from fbx
        // animations = Resources.LoadAll<AnimationClip>("my_fbx");

        ReadAnimationData();

        //record the current pose as the "origin" pose
        //RecordTransformPositions();
        //RecordTransformScales();
        //RecordTransformRotations();
        ResetAnimation();
    }

    int f;
    // Update is called once per frame
    void FixedUpdate()
    {

        if (play && (f < fixedAnimations[0].GetLength()))
        {
            //Debug.Log(f);
            this.ApplyPosData(this.transform, fixedAnimations[0].GetFrameAt(f).deltaPos);
            this.ApplyScaleData(this.transform, fixedAnimations[0].GetFrameAt(f).deltaScale);
            this.ApplyRotData(this.transform, fixedAnimations[0].GetFrameAt(f).deltaRot);
            f++;

            if (f >= fixedAnimations[0].GetLength()) { ResetAnimation(); }
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            play = true;
        }

    }


    private void ResetAnimation()
    {
        //apply the "origin" pose to transform, deltas are based on that
        this.ApplyPosData(this.transform, allPositions);
        this.ApplyScaleData(this.transform, allScales);
        this.ApplyRotData(this.transform, allRotations);
        f = 0;
        play = false;
    }

    //reads clip data from animation controller and makes a new array of FixedAnimations
    private void ReadAnimationData()
    {
        AnimationClip[] animations = controller.animationClips;

        int len = animations.Length;

        List<FixedAnimation> animList = new List<FixedAnimation>();

        //record the current pose as the "origin" pose
        RecordTransformPositions();
        RecordTransformScales();
        RecordTransformRotations();

        //iterate through each animation
        for (int i = 0; i < len; i++)
        {
            //holds the current animation clip for easy access
            AnimationClip hold = animations[i];

            //time data to sample the animation data
            float frameRate = Time.fixedDeltaTime;
            float totalTime = 0f;
            float endTime = hold.length;

            //apply the "origin" pose to transform, deltas are based on that
            this.ApplyPosData(this.transform, allPositions);
            this.ApplyScaleData(this.transform, allScales);
            this.ApplyRotData(this.transform, allRotations);

            //list of deltas on each frame to be used to creat the new animation
            List<FixedFrame> frameList = new List<FixedFrame>();


            //get the individual "frames" of animation by sampling the time passed
            while (totalTime < endTime)
            {

                //previous transform data from the last frame
                Vector3[] prevPos = this.RecurGetPos(this.transform);
                Vector3[] prevScale = this.RecurGetScale(this.transform);
                Quaternion[] prevRot = this.RecurGetRot(this.transform);

                //samples the animation to the transform
                hold.SampleAnimation(this.gameObject, totalTime);

                //previous transform data from the this frame
                Vector3[] curPos = this.RecurGetPos(this.transform);
                Vector3[] curScale = this.RecurGetScale(this.transform);
                Quaternion[] curRot = this.RecurGetRot(this.transform);

                //record deltas
                Vector3[] deltaPos = this.GetDeltaPos(curPos, prevPos);
                Vector3[] deltaScale = this.GetDeltaScale(curScale, prevScale);
                Quaternion[] deltaRot = this.GetDeltaRot(curRot, prevRot);

                //use deltas to create new frame
                FixedFrame newFrame = new FixedFrame(deltaPos, deltaScale, deltaRot);

                //add new frame to list
                frameList.Add(newFrame);

                //add a "frame" of time
                totalTime += frameRate;
            }

            //construct the new animation from the frames and add it to the list
            animList.Add(new FixedAnimation(frameList.ToArray()));


        }

        //turn the list to the array and BOOM, ya got all of your animations
        fixedAnimations = animList.ToArray();

    }

    private void RecordTransformPositions()
    {
        allPositions = this.RecurGetPos(this.transform);
    }
    private void RecordTransformScales()
    {
        allScales = this.RecurGetScale(this.transform);
    }

    private void RecordTransformRotations()
    {
        allRotations = this.RecurGetRot(this.transform);
    }

    private void ApplyPosData(Transform obj, Vector3[] pos)
    {
        this.ApplyPosDataToTransform(obj, pos, 0);
    }
    private void ApplyScaleData(Transform obj, Vector3[] scale)
    {
        this.ApplyScaleDataToTransform(obj, scale, 0);
    }
    private void ApplyRotData(Transform obj, Quaternion[] rot)
    {
        this.ApplyDataRotToTransform(obj, rot, 0);
    }

    private int ApplyPosDataToTransform(Transform obj, Vector3[] pos, int index)
    {        //just in case we somehow ended up with more objects than positions
        if (index >= pos.Length) { return index; }

        //set the position of the object
        obj.localPosition = pos[index];
        //Debug.Log(pos[index] + " | " + obj.localPosition + " -- " + index + " " + obj.gameObject.name);

        int len = obj.childCount;
        int childIndex = index;
        if (len > 0)
        {
            //Debug.Log(len);

            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);

                int ind = childIndex + 1;
                childIndex = this.ApplyPosDataToTransform(child, pos, ind);

            }
        }

        return childIndex;
    }

    private int ApplyScaleDataToTransform(Transform obj, Vector3[] scale, int index)
    {
        //just in case we somehow ended up with more objects than positions
        if (index >= scale.Length) { return index; }

        //set the position of the object
        obj.localScale = scale[index];
        //Debug.Log(scale[index] + " | " + obj.localPosition + " -- " + index + " " + obj.gameObject.name);

        int len = obj.childCount;
        int childIndex = index;
        if (len > 0)
        {
            //Debug.Log(len);

            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);

                int ind = childIndex + 1;
                childIndex = this.ApplyScaleDataToTransform(child, scale, ind);

            }
        }

        return childIndex;
    }

    private int ApplyDataRotToTransform(Transform obj, Quaternion[] rot, int index)
    {
        //just in case we somehow ended up with more objects than rotations
        if (index >= rot.Length) { return index; }

        //set the rotation of the object
        obj.localRotation = rot[index];


        int len = obj.childCount;
        int childIndex = index;
        if (len > 0)
        {
            //Debug.Log(len);

            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);

                int ind = childIndex + 1;
                childIndex = this.ApplyDataRotToTransform(child, rot, ind);

            }
        }

        return childIndex;
    }

    //recursively called to return the position of each child in the transform
    //DFS recording with parent transform recorded before childeren
    private Vector3[] RecurGetPos(Transform obj)
    {
        List<Vector3> hold = new List<Vector3>();
        hold.Add(obj.localPosition);

        int len = obj.childCount;
        if (len > 0)
        {
            //Debug.Log(len);

            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);

                hold.AddRange(this.RecurGetPos(child));

            }
        }
        Vector3[] ret = hold.ToArray();
        return ret;
    }

    //recursively called to return the scale of each child in the transform
    //DFS recording with parent transform recorded before childeren
    private Vector3[] RecurGetScale(Transform obj)
    {
        List<Vector3> hold = new List<Vector3>();
        hold.Add(obj.localScale);

        int len = obj.childCount;
        if (len > 0)
        {
            //Debug.Log(len);

            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);

                hold.AddRange(this.RecurGetScale(child));

            }
        }
        Vector3[] ret = hold.ToArray();
        return ret;
    }

    //recursively called to return the rotation of each child in the transform
    //DFS recording with parent transform recorded before childeren
    private Quaternion[] RecurGetRot(Transform obj)
    {
        List<Quaternion> hold = new List<Quaternion>();
        hold.Add(obj.localRotation);

        int len = obj.childCount;
        if (len > 0)
        {
            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);

                hold.AddRange(this.RecurGetRot(child));

            }
        }
        Quaternion[] ret = hold.ToArray();
        return ret;
    }

    //for recording all the delta for the position to be made into a new frame
    private Vector3[] GetDeltaPos(Vector3[] cur, Vector3[] prev)
    {
        return GetDeltaPosRecur(cur, prev, 0);
    }


    //for recording all the delta for the Scale to be made into a new frame
    //same method as position, so we just use the position back-end for this
    private Vector3[] GetDeltaScale(Vector3[] cur, Vector3[] prev)
    {
        return GetDeltaScaleRecur(cur, prev, 0);
    }


    //actually records all the delta for the position to be made into a new frame
    //records the change in position by current-previous
    private Vector3[] GetDeltaPosRecur(Vector3[] cur, Vector3[] prev, int index)
    {
        if (cur.Length != prev.Length) { Debug.LogError("Number of position deltas mismatched, something went wrong"); }
        //precautionary return statement to prevent out of bounds
        else if (index >= prev.Length) { return new Vector3[0]; }

        List<Vector3> hold = new List<Vector3>();
        int len = prev.Length;

        for (int i = 0; i < len; i++)
        {
            Vector3 curPos = cur[i];
            Vector3 prevPos = prev[i];

            hold.Add((curPos - prevPos));
        }

        Vector3[] ret = hold.ToArray();
        return ret;

    }

    //actually records all the delta for the position to be made into a new frame
    //records the change in position by current-previous
    private Vector3[] GetDeltaScaleRecur(Vector3[] cur, Vector3[] prev, int index)
    {
        if (cur.Length != prev.Length) { Debug.LogError("Number of position deltas mismatched, something went wrong"); }
        //precautionary return statement to prevent out of bounds
        else if (index >= prev.Length) { return new Vector3[0]; }

        List<Vector3> hold = new List<Vector3>();
        int len = prev.Length;

        for (int i = 0; i < len; i++)
        {
            Vector3 curPos = cur[i];
            Vector3 prevPos = prev[i];

            hold.Add((curPos - prevPos));
        }

        Vector3[] ret = hold.ToArray();
        return ret;

    }

    //for recording all the delta for the rotation to be made into a new frame
    private Quaternion[] GetDeltaRot(Quaternion[] cur, Quaternion[] prev)
    {
        return GetDeltaRotRecur(cur, prev, 0);
    }


    //actually records all the delta for the rotation to be made into a new frame
    //records the change in rotation by current-previous
    private Quaternion[] GetDeltaRotRecur(Quaternion[] cur, Quaternion[] prev, int index)
    {
        if (cur.Length != prev.Length) { Debug.LogError("Number of rotation deltas mismatched, something went wrong"); }
        //precautionary return statement to prevent out of bounds
        else if (index >= prev.Length) { return new Quaternion[0]; }

        List<Quaternion> hold = new List<Quaternion>();
        int len = prev.Length;

        for (int i = 0; i < len; i++)
        {
            Quaternion curRot = cur[i];
            Quaternion prevRot = prev[i];

            Quaternion toAdd = new Quaternion(curRot.x - prevRot.x, curRot.y - prevRot.y, curRot.z - prevRot.z, curRot.w - prevRot.w);

            hold.Add(toAdd);
        }

        Quaternion[] ret = hold.ToArray();
        return ret;

    }


    //for applying the fixed vector values when playing the animation
    private void ApplyPosData(Transform obj, FVector3[] pos)
    {
        this.ApplyPosDataToTransform(obj, pos, 0);
    }
    private void ApplyScaleData(Transform obj, FVector3[] scale)
    {
        this.ApplyScaleDataToTransform(obj, scale, 0);
    }
    private void ApplyRotData(Transform obj, FVector4[] rot)
    {
        this.ApplyDataRotToTransform(obj, rot, 0);
    }

    //for playing the fixed animations
    private int ApplyPosDataToTransform(Transform obj, FVector3[] pos, int index)
    {
        //just in case we somehow ended up with more objects than positions
        if (index >= pos.Length) { return 0; }

        //set the position of the object
        FVector3 delta = pos[index];
        obj.localPosition += new Vector3((float)delta.x, (float)delta.y, (float)delta.z);

        int len = obj.childCount;
        int childIndex = index;
        if (len > 0)
        {
            //Debug.Log(len);

            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);

                int ind = childIndex + 1;
                childIndex = this.ApplyPosDataToTransform(child, pos, ind);

            }
        }

        return childIndex;
    }

    private int ApplyScaleDataToTransform(Transform obj, FVector3[] scale, int index)
    {
        //just in case we somehow ended up with more objects than positions
        if (index >= scale.Length) { return 0; }

        //set the position of the object
        FVector3 delta = scale[index];
        obj.localScale += new Vector3((float)delta.x, (float)delta.y, (float)delta.z);

        int len = obj.childCount;
        int childIndex = index;
        if (len > 0)
        {
            //Debug.Log(len);

            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);

                int ind = childIndex + 1;
                childIndex = this.ApplyScaleDataToTransform(child, scale, ind);

            }
        }

        return childIndex;
    }

    private int ApplyDataRotToTransform(Transform obj, FVector4[] rot, int index)
    {
        //just in case we somehow ended up with more objects than rotations
        if (index >= rot.Length) { return 0; }

        //set the rotation of the object
        FVector4 delta = rot[index];

        Quaternion curRot = obj.localRotation;
        Quaternion newRot = new Quaternion((float)delta.x + curRot.x, (float)delta.y + curRot.y, (float)delta.z + curRot.z, (float)delta.w + curRot.w);
        obj.localRotation = newRot;
        //Debug.Log(obj.localRotation + " : " + newRot + " | " + obj.gameObject.name + " " + index);
        int len = obj.childCount;
        int childIndex = index;
        if (len > 0)
        {
            //Debug.Log(len);

            for (int i = 0; i < len; i++)
            {
                Transform child = obj.GetChild(i);
                int ind = childIndex + 1;
                childIndex = this.ApplyDataRotToTransform(child, rot, ind);

            }
        }
        return childIndex;
    }
}
