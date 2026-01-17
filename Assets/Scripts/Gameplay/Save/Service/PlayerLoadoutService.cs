using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Save.Service
{
    public sealed class PlayerLoadoutService
    {
        public PlayerLoadoutModel Current { get; private set; } = new();

        public void Configure(PlayerLoadoutModel loadoutModel)
        {
            Current = loadoutModel;
        }
    }
}