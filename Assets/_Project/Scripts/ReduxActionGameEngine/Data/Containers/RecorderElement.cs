
namespace ActionGameEngine.Input
{
    [System.Serializable]
    public struct RecorderElement
    {

        public InputFragment frag;
        public int framesHeld;

        public InputItem item
        {
            get => frag.inputItem;
        }

        public RecorderElement(InputItem newItem)
        {
            frag = new InputFragment(newItem);
            framesHeld = 0;
        }

        public bool IsEmpty()
        {
            return item.m_rawValue != 0;
        }
    }
}