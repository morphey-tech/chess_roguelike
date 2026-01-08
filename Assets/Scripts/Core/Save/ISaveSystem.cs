using Cysharp.Threading.Tasks;

namespace Project.Core.Save
{
    public interface ISaveSystem
    {
        UniTask SaveAsync(string slotId);
        UniTask<bool> LoadAsync(string slotId);
        UniTask<bool> HasSaveAsync(string slotId);
        UniTask DeleteSaveAsync(string slotId);
        UniTask<string[]> GetAllSaveSlotsAsync();
    }
}


