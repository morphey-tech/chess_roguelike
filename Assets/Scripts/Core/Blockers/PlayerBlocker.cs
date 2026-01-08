using System;

namespace Project.Core.Core.Blockers
{
    [Flags]
    public enum PlayerBlocker
    {
        None = 0,
        MovementBlock = 1 << 0,
        LookBlock = 1 << 1,
        Cutscene = 1 << 2,
        Dialogue = 1 << 3,
    }
}