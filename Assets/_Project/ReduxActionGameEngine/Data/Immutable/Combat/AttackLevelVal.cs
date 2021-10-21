namespace ActionGameEngine.Data
{
    public struct AttackLevelVal
    {
        //hitstop to apply to hit target
        public int hitstop;
        //hitstop to apply to attacker
        public int hitstopSelf;
        //amount of hitstop to add on counterhit
        public int counterHitstop;
        public int standingStun;
        public int crouchingStun;
        public int airUntechTime;
        public int groundBlockstun;
        public int airBlockstun;

        public AttackLevelVal(int hStop, int hStopSelf, int chhStop, int sStun, int cStun, int airUntech, int gBlck, int aBlck)
        {
            hitstop = hStop;
            hitstopSelf = hStopSelf;
            counterHitstop = chhStop;
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
            standingStun = sStun;
            crouchingStun = cStun;
            airUntechTime = airUntech;
            groundBlockstun = gBlck;
            airBlockstun = aBlck;

        }
    }
}