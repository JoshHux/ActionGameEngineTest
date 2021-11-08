using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using FixedAnimationSystem;

public class FBXReader : MonoBehaviour
{
    public AnimatorController controller;
    public GameObject go;
    [SerializeField] private AnimationClip[] animations;
    [SerializeField] private FixedAnimation[] fixedAnimations;
    private Vector3 originPos;
    private Transform origin;

    public bool play = false;
    // Start is called before the first frame update
    void Start()
    {
        // animations = Resources.LoadAll<AnimationClip>("my_fbx");
        animations = controller.animationClips;
        this.ReadAnimationData();
        play = false;
        this.PlayAnimation();
    }
    int f = 0;
    void FixedUpdate()
    {
        if (f < fixedAnimations[0].frames.Length)
        {
            AssignTransformToChilderen(go.transform, fixedAnimations[0].frames[f], 0);
            f++;
        }
    }

    public void PlayAnimation()
    {
        go.transform.position = origin.position;
        go.transform.rotation = origin.rotation;
        AssignTransformToChilderen(go.transform, fixedAnimations[0].frames[0], 0);
        play = true;
        f = 0;
    }

    private void AssignTransformToChilderen(Transform parent, AnimFrame delta, int index)
    {
        Vector3 pos = new Vector3((float)delta.deltaPos[index].x, (float)delta.deltaPos[index].y, (float)delta.deltaPos[index].z);
        parent.position += pos;
        Quaternion rot = new Quaternion(parent.rotation.x + (float)delta.deltaRot[index].x, parent.rotation.y + (float)delta.deltaRot[index].y, parent.rotation.z + (float)delta.deltaRot[index].z, parent.rotation.w + (float)delta.deltaRot[index].w);
        parent.rotation = rot;
        //Debug.Log(pos + "  |  " + delta.deltaRot[index]);

        int len = parent.childCount;
        if (len > 0)
        {
            for (int i = 0; i < len; i++)
            {
                Transform cur = parent.GetChild(i);

                this.AssignTransformToChilderen(cur, delta, index + 1);
            }
        }

    }

    private void ReadAnimationData()
    {
        int len = animations.Length;

        List<FixedAnimation> animList = new List<FixedAnimation>();


        animations[0].SampleAnimation(this.go, 0f);

        Transform baseTransform = this.go.transform;
        origin = baseTransform;
        for (int i = 0; i < len; i++)
        {
            AnimationClip hold = animations[i];

            float frameRate = Time.deltaTime;
            float totalTime = 0f;
            float endTime = hold.length;
            Transform prevTransform = baseTransform;

            List<AnimFrame> frameList = new List<AnimFrame>();

            //get the individual frames of animation
            while (totalTime < endTime)
            {
                //samples the animation to get changes to go
                hold.SampleAnimation(this.go, totalTime);


                frameList.Add(this.SampleTransformFromObject(this.go.transform, prevTransform));

                prevTransform = this.go.transform;
                totalTime += frameRate;
            }
            animList.Add(new FixedAnimation(frameList.ToArray()));

            //int animFrames=(hold.length/Time.deltaTime);

        }

        fixedAnimations = animList.ToArray();

    }


    private AnimFrame SampleTransformFromObject(Transform cur, Transform prev)
    {
        Vector3[] deltaPos = this.GetPos(cur, prev);
        Quaternion[] deltaRot = this.GetRot(cur, prev);

        AnimFrame ret = new AnimFrame(deltaPos, deltaRot);

        return ret;
    }

    //gets the change in position for all transforms under and including parent
    //does this by prevParent.pos-parent.pos for everything
    //DFS search
    private Vector3[] GetDeltaPos(Transform parent, Transform prevParent)
    {
        List<Vector3> hold = new List<Vector3>();
        //add parent before anything else
        hold.Add(parent.position - prevParent.position);

        int len = parent.childCount;
        if (len > 0)
        {
            for (int i = 0; i < len; i++)
            {
                Transform cur = parent.GetChild(i);
                Transform prev = prevParent.GetChild(i);

                hold.AddRange(GetDeltaPos(cur, prev));

            }
        }
        Vector3[] ret = hold.ToArray();
        return ret;
    }


    private Vector3[] GetPos(Transform cur, Transform prev)
    {


        Vector3[] ret = this.GetDeltaPos(cur, prev);
        return ret;
    }

    private Quaternion[] GetDeltaRot(Transform parent, Transform prevParent)
    {
        List<Quaternion> hold = new List<Quaternion>();

        //add parent before anything else
        Quaternion toAdd = new Quaternion(parent.rotation.x - prevParent.rotation.x, parent.rotation.y - prevParent.rotation.y, parent.rotation.z - prevParent.rotation.z, parent.rotation.w - prevParent.rotation.w);
        hold.Add(toAdd);

        int len = parent.childCount;
        if (len > 0)
        {
            for (int i = 0; i < len; i++)
            {
                Transform cur = parent.GetChild(i);
                Transform prev = prevParent.GetChild(i);

                hold.AddRange(GetDeltaRot(cur, prev));

            }
        }
        Quaternion[] ret = hold.ToArray();
        return ret;
    }


    private Quaternion[] GetRot(Transform cur, Transform prev)
    {



        Quaternion[] ret = this.GetDeltaRot(cur, prev);
        return ret;
    }
}
