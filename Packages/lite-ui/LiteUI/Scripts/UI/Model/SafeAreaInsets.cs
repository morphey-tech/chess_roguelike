namespace LiteUI.UI.Model
{
    public class SafeAreaInsets
    {
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Right { get; private set; }
        public int Bottom { get; private set; }

        public SafeAreaInsets(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public override string ToString()
        {
            return "[SafeAreaInsets: Left: " + Left +
                   ", Top: " + Top +
                   ", Right: " + Right +
                   ", Bottom: " + Bottom + "]";
        }
    }
}
