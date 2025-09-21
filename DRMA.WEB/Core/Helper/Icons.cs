namespace DRMA.WEB.Core.Helper
{
    public enum IconSize
    {
        sm,
        md,
        lg,
    }

    public enum IconAnimation
    {
        None,
        Beat,
        BeatFade,
        Bounce,
        Fade,
        Flip,
        Shake,
        Spin,
        SpinReverse,
        SpinPulse
    }

    public class Icon(string category, string name, IconSize size = IconSize.md, IconAnimation animation = IconAnimation.None)
    {
        public string Category { get; set; } = category;
        public string Name { get; set; } = name;
        public IconSize Size { get; set; } = size;
        public IconAnimation Animation { get; set; } = animation;

        public Icon WithSise(IconSize size)
        {
            Size = size;
            return this;
        }

        public Icon WithAnimation(IconAnimation animation)
        {
            Animation = animation;
            return this;
        }

        public string Font => IconHelper.GetFont(this);
    }

    public static class IconHelper
    {
        public static string GetFont(Icon icon)
        {
            var sizeClass = icon.Size switch
            {
                IconSize.sm => "fa-sm",
                IconSize.md => "fa-md",
                IconSize.lg => "fa-lg",
                _ => string.Empty
            };
            var animationClass = icon.Animation switch
            {
                IconAnimation.Beat => "fa-beat",
                IconAnimation.BeatFade => "fa-beat-fade",
                IconAnimation.Bounce => "fa-bounce",
                IconAnimation.Fade => "fa-fade",
                IconAnimation.Flip => "fa-flip",
                IconAnimation.Shake => "fa-shake",
                IconAnimation.Spin => "fa-spin",
                IconAnimation.SpinReverse => "fa-spin-reverse",
                IconAnimation.SpinPulse => "fa-spin-pulse",
                _ => string.Empty
            };

            return $"{icon.Category} fa-{icon.Name} {sizeClass} {animationClass}".Trim();
        }
    }

    public static class IconsFA
    {
        public static class Solid
        {
            public static Icon Icon(string name) => new("fa-solid", name);
        }

        public static class Brands
        {
            public static Icon Icon(string name) => new("fa-brands", name);
        }
    }
}