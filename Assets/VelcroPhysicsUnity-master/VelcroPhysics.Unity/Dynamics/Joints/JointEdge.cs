namespace VelcroPhysics.Dynamics.VJoints
{
    /// <summary>
    /// A VJoint edge is used to connect bodies and VJoints together
    /// in a VJoint graph where each body is a node and each VJoint
    /// is an edge. A VJoint edge belongs to a doubly linked list
    /// maintained in each attached body. Each VJoint has two VJoint
    /// nodes, one for each attached body.
    /// </summary>
    public sealed class VJointEdge
    {
        /// <summary>
        /// The VJoint.
        /// </summary>
        public VJoint VJoint;

        /// <summary>
        /// The next VJoint edge in the body's VJoint list.
        /// </summary>
        public VJointEdge Next;

        /// <summary>
        /// Provides quick access to the other body attached.
        /// </summary>
        public Body Other;

        /// <summary>
        /// The previous VJoint edge in the body's VJoint list.
        /// </summary>
        public VJointEdge Prev;
    }
}