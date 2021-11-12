using System.Collections.Generic;
using FixMath.NET;

namespace FixedAnimationSystem
{
    [System.Serializable]
    public struct FixedFrame
    {
        public FVector3[] deltaPos;

        public FVector3[] deltaScale;
        //FVector4 being used as a ghetto Quaternion
        //essentially used as a container for the rotation of all bones
        public FVector4[] deltaRot;

        public IAnimationEvent[] events;

        public FixedFrame(FVector3[] deltaPos, FVector3[] deltaScale, FVector4[] deltaRot)
        {
            this.deltaPos = deltaPos;
            this.deltaScale = deltaScale;
            this.deltaRot = deltaRot;
            this.events = new IAnimationEvent[0];
        }

        public FixedFrame(UnityEngine.Vector3[] deltaPos, UnityEngine.Vector3[] deltaScale, UnityEngine.Quaternion[] deltaRot)
        {

            int len = deltaPos.Length;
            List<FVector3> holdPos = new List<FVector3>();

            for (int i = 0; i < len; i++)
            {
                UnityEngine.Vector3 hold = deltaPos[i];

                FVector3 toAdd = new FVector3((Fix64)hold.x, (Fix64)hold.y, (Fix64)hold.z);

                holdPos.Add(toAdd);
            }

            len = deltaScale.Length;
            List<FVector3> holdScale = new List<FVector3>();

            for (int i = 0; i < len; i++)
            {
                UnityEngine.Vector3 hold = deltaScale[i];

                FVector3 toAdd = new FVector3((Fix64)hold.x, (Fix64)hold.y, (Fix64)hold.z);

                holdScale.Add(toAdd);
            }


            len = deltaRot.Length;
            List<FVector4> holdRot = new List<FVector4>();

            for (int i = 0; i < len; i++)
            {
                UnityEngine.Quaternion hold = deltaRot[i];

                FVector4 toAdd = new FVector4((Fix64)hold.x, (Fix64)hold.y, (Fix64)hold.z, (Fix64)hold.w);

                holdRot.Add(toAdd);
            }

            this.deltaPos = holdPos.ToArray();
            this.deltaScale = holdScale.ToArray();
            this.deltaRot = holdRot.ToArray();
            this.events = new IAnimationEvent[0];
        }

        public static FixedFrame operator +(FixedFrame a, FixedFrame b)
        {
            //all deltas should have the same length, the number of transforms to keep track of doesn't change
            int len = a.deltaPos.Length;
            FVector3[] diffPos = new FVector3[len];
            FVector3[] diffScale = new FVector3[len];
            FVector4[] diffRot = new FVector4[len];

            for (int i = 0; i < len; i++)
            {
                diffPos[i] = a.deltaPos[i] + b.deltaPos[i];
                diffScale[i] = a.deltaScale[i] + b.deltaScale[i];
                diffRot[i] = a.deltaRot[i] + b.deltaRot[i];
            }

            FixedFrame ret = new FixedFrame(diffPos, diffScale, diffRot);
            return ret;
        }
    }
}
