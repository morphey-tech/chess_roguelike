using System;
using JetBrains.Annotations;
using LiteUI.Common.Extensions;

namespace LiteUI.Element.Tab
{
    [PublicAPI]
    public enum TabSelectionState
    {
        NORMAL,
        HIGHLIGHTED,
        PRESSED,
        DISABLED
    }

    public static class TabSelectionStateExtensions
    {
        public static string GetName(this TabSelectionState status)
        {
            string? statusName = Enum.GetName(typeof(TabSelectionState), status);
            if (statusName == null) {
                throw new NullReferenceException($"Incorrect status type {status}");
            }
            return statusName.UnderscoreToCamelCase();
        }
    }
}
