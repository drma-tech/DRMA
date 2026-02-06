using MudBlazor;

namespace DRMA.WEB.Core.Helper
{
    public sealed class Css
    {
        private readonly List<string> _classes = [];

        public Css Raw(string value)
        {
            _classes.Add(value);
            return this;
        }

        public Css Small(string prefix)
        {
            _classes.Add($"{prefix}-{SpaceSmall}");
            return this;
        }

        public Css Medium(string prefix)
        {
            _classes.Add($"{prefix}-{SpaceMedium}");
            return this;
        }

        public Css Large(string prefix)
        {
            _classes.Add($"{prefix}-{SpaceLarge}");
            return this;
        }

        public static implicit operator string(Css css) => string.Join(" ", css._classes);

        public static Css Build() => new();

        public static int SpaceSmall => AppStateStatic.Breakpoint <= Breakpoint.Xs ? 2 : 3;
        public static int SpaceMedium => AppStateStatic.Breakpoint <= Breakpoint.Xs ? 4 : 6;
        public static int SpaceLarge => AppStateStatic.Breakpoint <= Breakpoint.Xs ? 6 : 9;
    }
}