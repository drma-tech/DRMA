namespace DRMA.API.Core
{
    /// <summary>
    /// this API is free, so it's not a big problem to expose this data
    /// </summary>
    public static class TmdbOptions
    {
        public const string Section = "TMDB";

        public const string BaseUri = "https://api.themoviedb.org/3/";
        public const string BaseUriNew = "https://api.themoviedb.org/4/";
        public const string ApiKey = "745ee705ec04f3be69ba3e449609f430";
        public const string SmallPosterPath = "https://www.themoviedb.org/t/p/w300/";
        public const string LargePosterPath = "https://www.themoviedb.org/t/p/w154/";
        public const string OriginalPosterPath = "https://www.themoviedb.org/t/p/original/";
    }
}