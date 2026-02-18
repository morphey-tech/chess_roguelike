using System;
using System.Collections.Generic;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Save.Service
{
    public class PlayerRunStateService
    {
        public PlayerRunStateModel? Current { get; private set; }

        public bool HasRun => Current != null;

        public void StartNew(PlayerLoadoutModel loadoutModel,
            IEnumerable<string> figureTypeIds,
            string initialStageId)
        {
            int seed = HasRun ? Current!.Seed : (int)DateTime.Now.Ticks;
            Current = new PlayerRunStateModel
            {
                StageId = initialStageId,
                CurrentStageIndex = 0,
                KingHp = 100,
                Seed = seed,
                BoardCapacity = 0,
                UsedCapacity = 0,
                Figures = new List<FigureState>()
            };

            // Create UnitState for each figure type
            foreach (string typeId in figureTypeIds)
            {
                Current.AddFigure(typeId);
            }
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
