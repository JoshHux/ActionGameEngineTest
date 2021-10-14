using BEPUphysics.Entities.Prefabs;
using FixMath.NET;
using Spax;
using UnityEngine;

namespace BEPUUnity
{
    public class BEPUSphere : ShapeBase
    {
        [SerializeField]
        private Fix64 m_radius = Fix64.One;

        public Fix64 radius
        {
            get
            {
                return m_radius;
            }
            set
            {
                m_radius = value;
            }
        }

        protected override void OnBepuAwake()
        {
            m_entity = new Sphere(m_startPosition, m_radius, m_mass);
            m_entity.Orientation = m_startOrientation;
        }

        protected override void RenderUpdate()
        {
            base.RenderUpdate();
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = ((float) m_mass > 0) ? Color.green : Color.red;
            Gizmos
                .DrawWireSphere(new Vector3(transform.position.x,
                    transform.position.y,
                    transform.position.z),
                (float) m_radius);
        }
#endif
    }
}
