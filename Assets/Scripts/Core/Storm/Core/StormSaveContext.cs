using System;

namespace Project.Core.Core.Storm.Core
{
    [Serializable]
    public class StormSaveContext
    {
        public StormState State;
        public int CurrentLayer;
        public int StepInLayer;
        public int ActivationTurn;
        public int? FirstDamageTurn;
        public bool FirstDamageDealt;
    }
}