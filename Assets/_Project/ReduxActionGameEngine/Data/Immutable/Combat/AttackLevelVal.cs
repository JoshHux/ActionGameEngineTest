namespace ActionGameEngine.Data.Helpers
{
    public struct AttackLevelVal
    {
        //hitstop to apply to hit target
        public int hitstop;
        //hitstop to apply to attacker
        public int hitstopSelf;
        //amount of hitstop to add on counterhit
        public int counterHitstop;

        //amount of hitstum to add on counterhit
        public int counterHitstun;
        public int standingStun;
        public int crouchingStun;
        public int airUntechTime;
        public int groundBlockstun;
        public int airBlockstun;

        public AttackLevelVal(int hStop, int hStopSelf, int chhStop, int chhStun, int sStun, int cStun, int airUntech, int gBlck, int aBlck)
        {
            hitstop = hStop;
            hitstopSelf = hStopSelf;
            counterHitstop = chhStop;
            counterHitstun = chhStun;
            standingStun = sStun;
            crouchingStun = cStun;
            airUntechTime = airUntech;
            groundBlockstun = gBlck;
            airBlockstun = aBlck;

        }


        public AttackLevelVal(int hStop, int chhStop, int sStun, int cStun, int airUntech, int gBlck, int aBlck)
        {
            hitstop = hStop;
            hitstopSelf = hStop;
            counterHitstop = chhStop;
            counterHitstun = chhStop;
            standingStun = sStun;
            crouchingStun = cStun;
            airUntechTime = airUntech;
            groundBlockstun = gBlck;
            airBlockstun = aBlck;
        }

        public int GetBlockstun(bool isGrounded, bool isCrouching)
        {
            int ret = groundBlockstun;
            int mod = 0;

            if (isGrounded)
            {
                //has to be nested, otherwise, standing stun will always be air stun
                if (isCrouching)
                {
                    mod = crouchingStun;
                }
            }
            else
            {
                ret = airBlockstun;
            }

            return ret + mod;
        }

        public int GetHitstun(bool isGrounded, bool isCrouching, bool isCounterHit)
        {
            int ret = standingStun;
            int mod = 0;

            if (isGrounded)
            {
                //has to be nested, otherwise, standing stun will always be air stun
                if (isCrouching)
                {
                    mod = crouchingStun;
                }

                if (isCounterHit)
                {
                    mod += counterHitstun;
                }

            }
            else
            {
                ret = airUntechTime;
                if (isCounterHit)
                {
                    mod = airUntechTime;
                }
            }

            return ret + mod;
        }

        public int GetHitstopEnemy(bool isCounterHit)
        {
            return GetHitstop(isCounterHit, false);
        }

        public int GetHitstopSelf(bool isCounterHit)
        {
            return GetHitstop(isCounterHit, true);
        }

        private int GetHitstop(bool isCounterHit, bool forSelf)
        {
            int ret = hitstop;
            int mod = 0;

            if (forSelf)
            {
                ret = hitstopSelf;
            }

            if (isCounterHit)
            {
                mod += counterHitstop;
            }

            return ret + mod;
        }
    }
}