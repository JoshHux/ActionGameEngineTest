using ActionGameEngine.Enum;
namespace ActionGameEngine.Data
{
    public struct CharacterStatus
    {
        public StateData currentState;
        public int currentHp;
        public StateCondition persistentCond;
        //if we need to check for a new state because of a new input
        public bool checkState;

    }
}