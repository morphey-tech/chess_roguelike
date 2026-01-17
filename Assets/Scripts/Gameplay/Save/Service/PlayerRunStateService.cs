using System;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Save.Service
{
    public class PlayerRunStateService
    {
        public PlayerRunStateModel? Current { get; private set; }

        public bool HasRun => Current != null;

        public void StartNew(PlayerLoadoutModel loadoutModel)
        {
            int seed = HasRun ? Current!.Seed : (int)DateTime.Now.Ticks;
            Current = new PlayerRunStateModel
            {
                StageId = "1",
                KingHp = 100,
                Seed = seed
            };
        }

        public void Configure(PlayerRunStateModel runStateModel)
        {
            Current = runStateModel;
        }

        public void Clear()
        {
            Current = null;
        }
    }
}