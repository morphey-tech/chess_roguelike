using System;
using System.Collections.Generic;
using Project.Gameplay.Gameplay.Visual.Commands;

namespace Project.Gameplay.Gameplay.Combat.Visual
{
    public interface IVisualEventMapper
    {
        Type EventType { get; }
        IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent);
    }
}
