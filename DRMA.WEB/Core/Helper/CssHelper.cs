namespace DRMA.WEB.Core.Helper
{
    public sealed class CssHelper
    {
        private readonly List<string> _classes = [];

        public CssHelper Raw(string value)
        {
            _classes.Add(value);
            return this;
        }

        public CssHelper Small(string prefix)
        {
            _classes.Add($"{prefix}-{SpaceSmall}");
            return this;
        }

        public CssHelper Medium(string prefix)
        {
            _classes.Add($"{prefix}-{SpaceMedium}");
            return this;
        }

        public CssHelper Large(string prefix)
        {
            _classes.Add($"{prefix}-{SpaceLarge}");
            return this;
        }

        public static implicit operator string(CssHelper css) => string.Join(" ", css._classes);

        public static CssHelper Build() => new();

        public static int SpaceSmall => AppStateStatic.IsMobile ? 2 : 3;
        public static int SpaceMedium => AppStateStatic.IsMobile ? 4 : 6;
        public static int SpaceLarge => AppStateStatic.IsMobile ? 6 : 9;
    }
}