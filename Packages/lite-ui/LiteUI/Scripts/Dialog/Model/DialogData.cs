using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LiteUI.Dialog.Model
{
    internal class DialogData
    {
        public Type DialogType { get; }
        public object?[]? Params { get; }
        public UniTaskCompletionSource<MonoBehaviour>? ShowCompletionSource { get; }

        public DialogData(Type dialogType, object?[]? parameters, UniTaskCompletionSource<MonoBehaviour>? showCompletionSource)
        {
            DialogType = dialogType;
            Params = parameters;
            ShowCompletionSource = showCompletionSource;
        }
    }
}
