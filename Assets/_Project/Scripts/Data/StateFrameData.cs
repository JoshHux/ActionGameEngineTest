
using Spax.Input;


namespace Spax.StateMachine
{
    [System.Serializable]
    public class StateFrameData
    {
        public string stateName = "New State";
        public uint stateID;
        public int parentID;

        public EnterStateConditions enterStateConditions;
        public ExitStateConditions exitStateConditions;
        public StateConditions stateConditions;
        public CancelCondition cancelCondition = (CancelCondition)int.MaxValue;
        public Transition[] _transitions;

        public int duration;
        public CharacterFrame[] Frames;

        void OnValidate()
        {
            PrepFrames();

            //commandList.Prepare();
        }

        private void PrepFrames()
        {
            int len = Frames.Length;

            for (int i = 0; i < len; i++)
            {
                Frames[i].Prepare();
            }
        }

        //for finding the frame wanted
        //we do this so we minimize the number of frames that have no flags
        public bool FindFrame(int framesElapsed, ref CharacterFrame assign)
        {
            framesElapsed -= 1;
            //think about chaning to dictionary using atFrame as key, if dictionary is more performant
            //get length of array
            int len = Frames.Length;
            for (int i = 0; i < len; i++)
            {
                if (Frames[i].atFrame == framesElapsed)
                {
                    assign = Frames[i];
                    return true;
                }
            }

            return false;
        }





        public StateFrameData DeepCopy()
        {
            StateFrameData ret = new StateFrameData();
            ret.stateName = this.stateName;
            ret.stateID = this.stateID;
            ret.parentID = this.parentID;
            ret.enterStateConditions = this.enterStateConditions;
            ret.exitStateConditions = this.exitStateConditions;
            ret.stateConditions = this.stateConditions;
            ret.cancelCondition = this.cancelCondition;
            ret.duration = this.duration;

            //transitions is an array, so, I gotta do what I don't want to... Yay...
            int len = this._transitions.Length;
            Transition[] newArray = new Transition[len];
            for (int i = 0; i < len; i++)
            {
                newArray[i] = _transitions[i].DeepCopy();
            }
            ret._transitions = newArray;


            //WHY DID I MAKE SO MANY OF THESE ARRAYS UGH...
            len = this.Frames.Length;
            CharacterFrame[] newArrayFrames = new CharacterFrame[len];
            for (int i = 0; i < len; i++)
            {
                newArrayFrames[i] = Frames[i].DeepCopy();
                
            }
            ret.Frames = newArrayFrames;

            return ret;
        }
    }


}