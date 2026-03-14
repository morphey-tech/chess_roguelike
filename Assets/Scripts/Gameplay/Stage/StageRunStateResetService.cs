using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Figures.StatusEffects;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Encapsulates run-state mutations required before stage restart/switch.
    /// </summary>
    public sealed class StageRunStateResetService
    {
        public void ResetForStage(PlayerRunStateModel runState, string stageId)
        {
            runState.StageId = stageId;
            ResetFiguresToHand(runState);
        }

        public void ResetFiguresToHand(PlayerRunStateModel runState)
        {
            foreach (FigureState figure in runState.Figures)
            {
                figure.Location = FigureLocation.InHand();
            }
            runState.UsedCapacity = 0;
        }

        public void CollectFigureStates(PlayerRunStateModel runState, IEnumerable<Figure> figures)
        {
            foreach (Figure figure in figures)
            {
                FigureState? state = runState.GetFigure(figure.Id.ToString());
                if (state == null)
                {
                    continue;
                }

                // Сохраняем HP
                state.CurrentHp = (int)figure.Stats.CurrentHp.Value;
                state.MaxHp = figure.Stats.MaxHp;

                // Сохраняем статы с модификаторами
                state.Attack = figure.Stats.Attack.Value;
                state.Defence = figure.Stats.Defence.Value;
                state.Evasion = figure.Stats.Evasion.Value;

                // Сохраняем пассивки
                state.PassiveIds = figure.BasePassives.Select(p => p.Id).ToList();

                // Сохраняем статус-эффекты
                state.Effects = CollectEffectStates(figure.Effects.GetEffects());
            }
        }

        private static List<EffectState> CollectEffectStates(IEnumerable<IStatusEffect> effects)
        {
            List<EffectState> result = new();
            foreach (IStatusEffect effect in effects)
            {
                EffectState effectState = new(
                    effect.Id,
                    GetRemainingTurns(effect),
                    GetRemainingUses(effect),
                    GetStackCount(effect));
                result.Add(effectState);
            }

            return result;
        }

        private static int GetRemainingTurns(IStatusEffect effect)
        {
            if (effect is StatusEffectBase baseEffect)
            {
                FieldInfo? field = typeof(StatusEffectBase).GetField("RemainingTurns",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                return field != null ? (int)field.GetValue(baseEffect) : -1;
            }
            return -1;
        }

        private static int GetRemainingUses(IStatusEffect effect)
        {
            if (effect is StatusEffectBase baseEffect)
            {
                FieldInfo? field = typeof(StatusEffectBase).GetField("RemainingUses",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                return field != null ? (int)field.GetValue(baseEffect) : -1;
            }
            return -1;
        }

        private static int GetStackCount(IStatusEffect effect)
        {
            if (effect is StackableStatusEffect stackable)
            {
                return stackable.Stacks;
            }
            return 1;
        }
    }
}
