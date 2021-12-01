namespace ActionGameEngine.Interfaces
{
    public interface IAlligned
    {
        void SetAllignment(int allignment);
        int GetAllignment();

        bool IsActive();
    }
}