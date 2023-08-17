namespace ES.Common.Interfaces
{
    public interface IFindReplace
    {
        int FindReplacesCount { get; }
        IFindReplace GetFindReplace(int index);
    }
}
