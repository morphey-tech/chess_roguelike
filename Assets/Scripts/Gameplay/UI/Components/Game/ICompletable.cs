using System;
using UnityEngine;


namespace Project.Unity.UI.Components.Game
{
    public interface ICompletable
    {
        Component Value { get; }
        void SetOnCompleteAction(Action<ICompletable> action);
    }
}