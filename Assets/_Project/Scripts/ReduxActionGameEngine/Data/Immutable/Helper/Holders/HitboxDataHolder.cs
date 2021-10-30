using ActionGameEngine.Enum;
namespace ActionGameEngine.Data.Helpers
{
    //holder for gameplay so that we can easily access all the values we need to
    //only really meant to be read by a VulnerableObject
    [System.Serializable]
    public struct HitboxDataHolder
    {
        public int hitStopEnemy;
        public int hitStopSelf;
        //if airborne, this is untech time
        public int stun;

        //when you hit a move, what to apply to the hit entity
        public StateCondition hitCause;

        public HitType type;
    }
}
