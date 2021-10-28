namespace ActionGameEngine.Data.Helpers.Wrappers
{
    [System.Serializable]

    public struct StateListWrapper
    {
        public StateData[] stateList;

        public StateData this[int key]
        {
            get => stateList[key];
        }

    }
}
