using LiteUI.UI.Model;

namespace LiteUI.UI.Event
{
    public class SafeAreaEvent
    {
        public const string BANNER_AD_CHANGED = "safeAreaBannerAdChanged";
        public const string CHANGED = "safeAreaChanged";

        public SafeAreaInsets? Insets { get; }

        public SafeAreaEvent(SafeAreaInsets? insets = null)
        {
            Insets = insets;
        }
    }
}
