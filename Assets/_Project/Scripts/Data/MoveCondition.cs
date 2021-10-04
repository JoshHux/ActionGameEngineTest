    using FixMath.NET;

    [System.Serializable]
    public struct MoveCondition
    {
        //how many air jumps the character has done
        public int curAirJumps;

        //1 if facing right, -1 if facing left
        public bool facingRight;

        //is the character grounded?
        public bool isGrounded;
    }
