using FixMath.NET;
[System.Serializable]
public struct MoveStats
{
    //mass, multiply by gravity to get acceleration to the ground
    //sort of like a gravity scale
    public Fix64 mass;

    //max speed on the ground
    public Fix64 maxGroundSpeed;
    //max speed while walking on the ground
    public Fix64 maxWalkSpeed;

    //max speed in the air
    public Fix64 maxAirSpeed;

    //sef-explanitory
    public Fix64 maxFallSpeed;

    //how fast the character accelerates when they want to move on the ground
    public Fix64 groundAcceleration;
    //how fast the character accelerates when they want to move while walking on the ground
    public Fix64 walkAcceleration;

    //how fast the character accelerates when they want to move in the air
    public Fix64 airAcceleration;

    //how fast the character decelerates when on the ground
    public Fix64 groundFriction;

    //how fast the character decelerates when in the air
    public Fix64 airFriction;

    //how fast the character accelerates when they want to move on the ground
    public Fix64 jumpForce;

    //how many air jumps the character gets
    public int maxAirJumps;

    public MoveStats DeepCopy()
    {
        MoveStats ret = new MoveStats();
        ret.mass = this.mass;
        ret.maxGroundSpeed = this.maxGroundSpeed;
        ret.maxWalkSpeed = this.maxWalkSpeed;
        ret.maxAirSpeed = this.maxAirSpeed;
        ret.maxFallSpeed = this.maxFallSpeed;
        ret.groundAcceleration = this.groundAcceleration;
        ret.walkAcceleration = this.walkAcceleration;
        ret.airAcceleration = this.airAcceleration;
        ret.groundFriction = this.groundFriction;
        ret.airFriction = this.airFriction;
        ret.jumpForce = this.jumpForce;
        ret.maxAirJumps = this.maxAirJumps;
        return ret;
    }
}