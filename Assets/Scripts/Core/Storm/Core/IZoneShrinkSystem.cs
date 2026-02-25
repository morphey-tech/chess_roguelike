namespace Project.Core.Core.Storm.Core
{
    public interface IStormQueryService
    {
        StormCellStatus GetCellStatus(int row, int col);
        StormState GetCurrentState();
        int GetActivationTurn();
    }
}
