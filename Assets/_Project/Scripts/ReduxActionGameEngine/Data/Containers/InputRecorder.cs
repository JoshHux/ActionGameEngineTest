using System.Collections.Generic;
using ActionGameEngine.Enum;
namespace ActionGameEngine.Input
{
    //records the changes in the player's inputs
    [System.Serializable]
    public class InputRecorder
    {
        //max number of input changes to remember
        private readonly int maxLen = 255;
        //input leniency of 4 frames
        private readonly int leniency = 4;
        //records the inputs backwards
        private LinkedList<RecorderElement> inputChanges;

        //to remember the last whole input
        private InputItem prevItem;

        public InputRecorder()
        {
            prevItem = new InputItem();
            inputChanges = new LinkedList<RecorderElement>();
            //add at least one item to make it play nice
            inputChanges.AddLast(new RecorderElement());
        }

        //returns true if new element is added to vector or if the framesHeld on that element is <=leniencey
        public bool BufferInput(InputItem newInput)
        {

            bool ret = false;
            //if (newInput.m_rawValue > 0)
            //    UnityEngine.Debug.Log(prevItem.m_rawValue + " " + newInput.m_rawValue);
            //do we need to buffer change?
            if (prevItem != newInput)
            {
                //yes
                //let's buffer
                this.BufferChange(newInput);
            }


            //increment the frames held
            this.IncrementLast();
            //check leniency

            ret = (inputChanges.Last.Value.framesHeld <= leniency);



            return ret;
        }

        public RecorderElement[] GetInputArray()
        {
            RecorderElement[] ret = new RecorderElement[inputChanges.Count + 1];
            //buffers the current element as the first item, so we can check the current state directly
            inputChanges.CopyTo(ret, 0);
            System.Array.Reverse(ret);
            ret[0] = new RecorderElement(prevItem);
            /*string thing = "";
            foreach (var item in ret)
            {
                thing += " , " + item.frag.flags + " " + item.frag.inputItem.m_rawValue + " " + item.framesHeld;
            }
            UnityEngine.Debug.Log(thing);
            */
            return ret;
        }

        //only called if there is a change
        private void BufferChange(InputItem newItem)
        {
            if (inputChanges.Count > 0)
            {
                //remember the last whole item and compare that to the new item

                //find the released inputs
                RecorderElement released = new RecorderElement(InputItem.FindReleased(newItem, prevItem));
                //if there is anything to buffer
                if (!released.IsEmpty())
                {
                    //UnityEngine.Debug.Log("released :: " + prevItem.m_rawValue + " " + newItem.m_rawValue + " " + released.frag.inputItem.m_rawValue);

                    released.frag.flags |= InputFlags.RELEASED;
                    this.BufferElement(released);
                }
                //find the pressed elements
                RecorderElement pressed = new RecorderElement(InputItem.FindPressed(newItem, prevItem));
                //if there is anything to buffer
                if (!pressed.IsEmpty())
                {
                    //UnityEngine.Debug.Log(prevItem.m_rawValue + " " + newItem.m_rawValue + " " + pressed.frag.inputItem.m_rawValue);

                    pressed.frag.flags |= InputFlags.PRESSED;
                    //UnityEngine.Debug.Log(pressed.frag.inputItem.m_rawValue);
                    this.BufferElement(pressed);
                }

                //replace the last whole item
                prevItem = newItem;
            }
        }

        //responsible for buffering element
        private void BufferElement(RecorderElement newElem)
        {
            //don't keep any invalid items
            if (inputChanges.First.Value.frag.inputItem.m_rawValue == 0)
            {
                inputChanges.RemoveFirst();
            }
            if (inputChanges.Count >= maxLen)
            {
                //if there are too many elements, just remove the first element (represents the oldest input)
                inputChanges.RemoveFirst();
            }
            //add new element to the end of the list
            inputChanges.AddLast(newElem);
        }

        //I hate this, but I have to decapitate and reattatch the most recent value to increment it
        private void IncrementLast()
        {
            if (inputChanges.Count > 0)
            {
                RecorderElement last = inputChanges.Last.Value;
                last.framesHeld++;
                inputChanges.RemoveLast();
                inputChanges.AddLast(last);
            }
        }



    }
}