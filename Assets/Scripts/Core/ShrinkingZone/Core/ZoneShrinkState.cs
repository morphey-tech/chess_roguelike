using System;

namespace Project.Core.Core.ShrinkingZone.Core
{
    /// <summary>
    /// Состояние для сохранения/загрузки
    /// </summary>
    [Serializable]
    public class ZoneShrinkState
    {
        public ZoneState State;
        public int CurrentLayer;
        public int StepInLayer;
        public int ActivationTurn;
        public int? FirstDamageTurn;
        public bool FirstDamageDealt;
    }
}