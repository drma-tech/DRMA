namespace DRMA.Shared.Enums;

public enum TtlCache
{
    OneHour = 60 * 60 * 1, // 3600 seconds
    SixHours = 60 * 60 * 6, // 21600 seconds
    HalfDay = 60 * 60 * 12, // 43200 seconds
    OneDay = 60 * 60 * 24, // 86400 seconds
    TwoDays = 60 * 60 * 24 * 2, // 172800 seconds
    HalfWeek = 60 * 60 * 24 * 3, // 259200 seconds
    OneWeek = 60 * 60 * 24 * 7, // 604800 seconds
    TwoWeeks = 60 * 60 * 24 * 14, // 1209600 seconds
    OneMonth = 60 * 60 * 24 * 30, // 2592000 seconds
    ThreeMonths = 60 * 60 * 24 * 30 * 3, // 7776000 seconds
    SixMonths = 60 * 60 * 24 * 30 * 6 // 15552000 seconds
}
