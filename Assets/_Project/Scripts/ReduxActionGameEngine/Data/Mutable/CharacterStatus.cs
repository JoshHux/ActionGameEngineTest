using ActionGameEngine.Enum;
namespace ActionGameEngine.Data
{
    [System.Serializable]
    public struct CharacterStatus
    {
        //int ID of the current state
        [UnityEngine.SerializeField]private StateData currentState;
        [UnityEngine.SerializeField]private int currentHp;
        [UnityEngine.SerializeField]private int currentArmorHits;
        //conditions that are from the state and that we keep track of toggling
        //we don't want to change the current state's conditions, so we just copy them and manipulate that
        private StateCondition currentStateCond;
        private CancelConditions currentCancelCond;
        private TransitionFlag currentTransitionFlags;

        //conditions that last despite transitioning to another state
        private StateCondition persistentCond;
        //if we need to check for a new state because of a new input
        private bool checkState;
        private bool inHitstop;
        public void SetNewState(StateData newState) { currentState = newState; }
        public void SetNewStateConditions(StateCondition newCond) { currentStateCond = newCond; }
        public void ToggleStateConditions(StateCondition newCond) { currentStateCond ^= newCond; }
        public void SetPersistenConditions(StateCondition newCond) { persistentCond = newCond; }
        public void SetNewCancelConditions(CancelConditions newCond) { currentCancelCond = newCond; }
        public void ToggleCancelConditions(CancelConditions newCond) { currentCancelCond ^= newCond; }
        public void AddCancelConditions(CancelConditions newCond) { currentCancelCond |= newCond; }
        public void SetNewTransitionFlags(TransitionFlag newFlags) { currentTransitionFlags = newFlags; }
        public void AddTransitionFlags(TransitionFlag newFlags) { currentTransitionFlags |= newFlags; }
        public void SetCheckState(bool newInput) { checkState = newInput; }
        public void SetInHitstop(bool hitstop) { inHitstop = hitstop; }
        public void SetCurrentHP(int newHP) { currentHp = newHP; }


        public void SubtractCurrentHP(int change) { currentHp -= change; }
        public void AddCurrentHP(int change) { currentHp += change; }
        public StateData GetCurrentState() { return currentState; }
        public int GetCurrentStateID() { return currentState.stateID; }
        public int GetCurrentHp() { return currentHp; }
        public bool GetCheckState() { return checkState; }
        public bool GetInHitstop() { return inHitstop; }
        public StateCondition GetStateConditions() { return currentStateCond | persistentCond; }
        public StateCondition GetPersistentConditions() { return persistentCond; }
        public CancelConditions GetCancelConditions() { return currentCancelCond; }
        public TransitionFlag GetTransitionFlags() { return currentTransitionFlags; }
        //check to get correct FrameData
        public FrameData GetFrame(int f) { return currentState.GetFrameAt(f); }


    }
}