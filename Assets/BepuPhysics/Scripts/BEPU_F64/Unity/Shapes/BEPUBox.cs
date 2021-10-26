using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using FixMath.NET;
using UnityEngine;

namespace BEPUUnity
{
    public class BEPUBox : ShapeBase
    {
        public BEPUutilities.BepuVector3
            scale = new BEPUutilities.BepuVector3(1, 1, 1);

        private Fix64 m_width = Fix64.One;

        private Fix64 m_height = Fix64.One;

        private Fix64 m_length = Fix64.One;

        protected override void OnBepuAwake()
        {
            m_width = scale.X;
            m_height = scale.Y;
            m_length = scale.Z;
            m_entity =
                new Box(m_startPosition, m_width, m_height, m_length, m_mass);
            m_entity.Orientation = m_startOrientation;
        }

        protected override void RenderUpdate()
        {
            base.RenderUpdate();
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = ((float)m_mass > 0) ? Color.green : Color.red;

            // max/min
            /*BepuVector3 halfOffset = new BepuVector3((float)m_width / 2, (float)m_height / 2, (float)m_length / 2);
            BepuVector3 min = transform.position + transform.rotation * -halfOffset;
            BepuVector3 max = transform.position + transform.rotation * halfOffset;

            BepuVector3 x = transform.rotation * new BepuVector3((float)m_width, 0, 0);
            BepuVector3 y = transform.rotation * new BepuVector3(0, (float)m_height, 0);
            BepuVector3 z = transform.rotation * new BepuVector3(0, 0, (float)m_length);

            // Bottom
            Gizmos.DrawLine(min, min + x);
            Gizmos.DrawLine(min + x, min + x + z);
            Gizmos.DrawLine(min + x + z, min + z);
            Gizmos.DrawLine(min + z, min);

            // Top
            Gizmos.DrawLine(max, max - x);
            Gizmos.DrawLine(max - x, max - x - z);
            Gizmos.DrawLine(max - x - z, max - z);
            Gizmos.DrawLine(max - z, max);

            // Side
            Gizmos.DrawLine(min, min + y);
            Gizmos.DrawLine(min + x, min + x + y);
            Gizmos.DrawLine(min + x + z, min + x + z + y);
            Gizmos.DrawLine(min + z, min + z + y);
*/
            //Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z), Vector3.one);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);


            Vector3 hold = new Vector3((float)scale.X, (float)scale.Y, (float)scale.Z);
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
            Gizmos.DrawWireCube(Vector3.zero, hold);
        }
#endif
    }
}
