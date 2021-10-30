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
            RecorderElement[] ret = new RecorderElement[inputChanges.Count];
            inputChanges.CopyTo(ret, 0);
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
                    released.frag.flags |= InputFlags.RELEASED;
                    this.BufferElement(released);
                }
                //find the pressed elements
                RecorderElement pressed = new RecorderElement(InputItem.FindPressed(newItem, prevItem));
                //if there is anything to buffer
                if (!pressed.IsEmpty())
                {
                    pressed.frag.flags |= InputFlags.PRESSED;
                    this.BufferElement(pressed);
                }

                //replace the last whole item
                prevItem = newItem;
            }
        }

        //responsible for buffering element
        private void BufferElement(RecorderElement newElem)
        {
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