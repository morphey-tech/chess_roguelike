using System.IO;
using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Save
{
    public sealed class FileSaveStorage
    {
        private readonly string _rootPath;
        private const string Extension = ".sav";

        public FileSaveStorage(string rootPath)
        {
            _rootPath = rootPath;
            Directory.CreateDirectory(_rootPath);
        }

        public async UniTask WriteAsync(string slotId, string json)
        {
            string path = GetPath(slotId);
            await File.WriteAllTextAsync(path, json);
        }

        public async UniTask<string> ReadAsync(string slotId)
        {
            return await File.ReadAllTextAsync(GetPath(slotId));
        }

        public bool Exists(string slotId)
        {
            return File.Exists(GetPath(slotId));
        }

        public void Delete(string slotId)
        {
            string path = GetPath(slotId);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public string[] GetAllSlots()
        {
            string[] files = Directory.GetFiles(_rootPath, $"*{Extension}");
            string[] result = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                result[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return result;
        }

        private string GetPath(string slotId)
        {
            return Path.Combine(_rootPath, slotId + Extension);
        }
    }
}