using System;

namespace Project.Core.Core.Storm.SaveLoad
{
    [Serializable]
    public class StormStateSerializable
    {
        public int State;
        public int CurrentLayer;
        public int StepInLayer;
        public int ActivationTurn;
        public int? FirstDamageTurn;
        public bool FirstDamageDealt;
    }
}