using FixMath.NET;

namespace ActionGameEngine.Data
{
    //static struct to keep all of our character's stats, move list, and other immutable data
    [System.Serializable]
    public struct CharacterData
    {
        public int maxHealth;

        public Fix64 mass;

        public StateData[] stateList;
        public CommandList moveList;
    }
}
