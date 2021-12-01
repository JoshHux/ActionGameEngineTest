using ActionGameEngine.Enum;
namespace ActionGameEngine.Data
{
    [System.Serializable]
    public struct CharacterStatus
    {
        //int ID of the current state
        public StateData currentState;
        public int currentHp;
        public int currentArmorHits;
        public int facing;

        //conditions that are from the state and that we keep track of toggling
        //we don't want to change the current state's conditions, so we just copy them and manipulate that
        public StateCondition currentStateCond;
        public CancelConditions currentCancelCond;
        public TransitionFlag currentTransitionFlags;

        //conditions that last despite transitioning to another state
        public StateCondition persistentCond;
        //if we need to check for a new state because of a new input
        public bool checkState;
        public bool inHitstop;
        public void SetNewState(StateData newState)
        {
            checkState = true;
            currentState = newState;
        }
        public void SetNewStateConditions(StateCondition newCond) { currentStateCond = newCond; }
        public void ToggleStateConditions(StateCondition newCond) { currentStateCond ^= newCond; }
        public void SetPersistenConditions(StateCondition newCond) { persistentCond = newCond; }
        public void AddPersistenConditions(StateCondition newCond) { persistentCond |= newCond; }
        public void RemovePersistenConditions(StateCondition newCond) { persistentCond &= (~newCond); }
        public void SetNewCancelConditions(CancelConditions newCond)
        {
            checkState = true;
            currentCancelCond = newCond;
        }
        public void ToggleCancelConditions(CancelConditions newCond)
        {
            checkState = true;
            currentCancelCond ^= newCond;
        }
        public void AddCancelConditions(CancelConditions newCond)
        {
            checkState = true;
            currentCancelCond |= newCond;
        }
        public void SetNewTransitionFlags(TransitionFlag newFlags)
        {
            checkState = true;
            currentTransitionFlags = newFlags;
        }
        public void AddTransitionFlags(TransitionFlag newFlags)
        {
            //UnityEngine.Debug.Log("transitions flagged");
            checkState = true;
            currentTransitionFlags |= newFlags;
        }

        public void RemoveTransitionFlags(TransitionFlag newFlags)
        {
            //UnityEngine.Debug.Log("transitions flagged");
            checkState = true;
            //get the flags to remove
            var flags = currentTransitionFlags & newFlags;
            //flip the flags to only get every other flag
            var mask = ~flags;
            //and the mask to remove the flags we want to remove
            //this method of removing flags is safe and ensures no flags are added or removed when we don't want them to
            currentTransitionFlags &= mask;
        }

        //we check the state if we either recieve a new input or want to check the state in the first place
        public void SetCheckState(bool newInput) { checkState = newInput | checkState; }
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