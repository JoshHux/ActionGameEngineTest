
namespace ActionGameEngine.Input
{
    [System.Serializable]
    //4+3*x bytes
    public struct Command
    {
        //state to transition to is the command input is met
        public int state;
        public InputFragment[] commandInput;

        public bool Check(InputFragment[] playerInputs)
        {
            int pos = 0;
            int len = playerInputs.Length;
            foreach (InputFragment frag in commandInput)
            {
                //we need to do an indexing for loop because we want to save our position as we move to the next frag
                for (int i = pos; i < len; i++)
                {
                    InputFragment input = playerInputs[i];
                    if (frag.Check(input))
                    {
                        //next starting position of the player's inputs
                        pos = i;
                        //breaks out of nested loop only
                        break;
                    }

                    //reached the end of the player's inputs, no reason to continue
                    //only reasched if player's inputs failed to match to command
                    if (i == len - 1)
                    {
                        return false;
                    }
                }

            }

            return true;
        }
    }
}