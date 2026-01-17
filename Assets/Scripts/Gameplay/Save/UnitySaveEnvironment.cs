using System.IO;
using Project.Core.Core.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Gameplay.Gameplay.Save
{
    public sealed class UnitySaveEnvironment : ISaveEnvironment
    {
        public string SavePath =>
            Path.Combine(Application.persistentDataPath, "Saves");

        public string CurrentScene =>
            SceneManager.GetActiveScene().name;
    }
}