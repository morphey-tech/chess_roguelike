using System;

namespace LiteUI.Common.Utils
{
    public static class TimeUtils
    {
        public static double CurrentTimestamp => DateTime.Now.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}
