using Cysharp.Threading.Tasks;

namespace Project.Core.Core.Save
{
    public interface ISaveService
    {
        UniTask SaveAsync(string slotId);
        UniTask<bool> LoadAsync(string slotId);
        UniTask<bool> HasSaveAsync(string slotId);
        UniTask DeleteAsync(string slotId);
        UniTask<string[]> GetSlotsAsync();
    }
}


