using System;
using System.Collections.Generic;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Prepare;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public static class StagePipelineConfig
    {
        private static readonly Dictionary<StageType, Type[]> Pipelines = new()
        {
            {
                StageType.Duel, new[]
                {
                    typeof(BoardSpawnPhase),
                    typeof(PreparePlacementPhase),  // Player places figures first
                    typeof(FiguresSpawnPhase),       // Enemies spawn after player is ready
                    typeof(GameplayInitPhase),
                    typeof(BattleDuelPhase)
                }
            }
        };

        public static Type[] GetPipeline(StageType type)
        {
            return Pipelines.TryGetValue(type, out Type[] pipeline)
                ? pipeline
                : Array.Empty<Type>();
        }
    }
}
