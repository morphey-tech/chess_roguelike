using System;
using UnityEngine.Events;

namespace Project.Unity.Unity.Bootstrap
{
    [Serializable]
    public class InitializationStep
    {
        public string Name;
        public bool Enabled = true;
        public float Weight = 1f;
        public float DelayBefore = 0f;
        public float DelayAfter = 0f;
        public bool ContinueOnError = false;
        public UnityEvent OnExecute;
    }
}