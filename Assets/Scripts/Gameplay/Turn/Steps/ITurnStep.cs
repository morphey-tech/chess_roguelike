using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Turn.Steps
{
    public interface ITurnStep
    {
        string Id { get; }
        UniTask ExecuteAsync(ActionContext context);
    }
}
