namespace DRMA.WEB.Modules.Support.Core
{
    public class ProductLink(string title, string subTitle, string url, string logo, bool live)
    {
        public string Title { get; set; } = title;
        public string SubTitle { get; set; } = subTitle;
        public string Url { get; set; } = url;
        public string Logo { get; set; } = logo;
        public bool Live { get; set; } = live;
    }
}