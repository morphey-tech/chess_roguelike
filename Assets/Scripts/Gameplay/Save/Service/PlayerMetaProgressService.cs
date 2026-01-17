using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Save.Service
{
    public sealed class PlayerMetaProgressService
    {
        public PlayerMetaProgressModel Model { get; private set; } = new();

        public void Configure(PlayerMetaProgressModel metaProgressModel)
        {
            Model = metaProgressModel;
        }
    }
}