using System.Collections.Generic;
using FixMath.NET;

namespace FixedAnimationSystem
{
    [System.Serializable]
    public class AnimFrame
    {
        public FVector3[] deltaPos;
        //FVector4 being used as a ghetto Quaternion
        //essentially used as a container for the rotation of all bones
        public FVector4[] deltaRot;

        public AnimFrame(UnityEngine.Vector3[] deltaPos, UnityEngine.Quaternion[] deltaRot)
        {

            int len = deltaPos.Length;
            List<FVector3> holdPos = new List<FVector3>();

            for (int i = 0; i < len; i++)
            {
                UnityEngine.Vector3 hold = deltaPos[i];

                FVector3 toAdd = new FVector3((Fix64)hold.x, (Fix64)hold.y, (Fix64)hold.z);

                holdPos.Add(toAdd);
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
            this.deltaRot = holdRot.ToArray();
        }
    }
}
