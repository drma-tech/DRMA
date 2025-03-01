namespace DRMA.WEB.Core
{
    /// <summary>
    /// this API is free, so it's not a big problem to expose this data
    /// </summary>
    public static class TmdbOptions
    {
        public const string Section = "TMDB";

        public const string BaseUri = "https://api.themoviedb.org/3/";
        public const string BaseUriNew = "https://api.themoviedb.org/4/";
        public const string ApiKey = "4e0dee3ee7786c86df1f434849570b55";
        public const string SmallPosterPath = "https://www.themoviedb.org/t/p/w185/";
        public const string LargePosterPath = "https://www.themoviedb.org/t/p/w300/";
        public const string OriginalPosterPath = "https://www.themoviedb.org/t/p/original/";
        public const string FacePath = "https://www.themoviedb.org/t/p/w66_and_h66_face/";
    }
}