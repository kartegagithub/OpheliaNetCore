namespace Ophelia.Web
{
    public enum DateTimeFormatType
    {
        DateOnly = 0,
        TimeOnly = 1,
        DateTimeWithHour = 2
    }
    public enum DateTimeType
    {
        None = 0,
        ShortDate = 1,
        LongDate = 2,
        ShortTime = 3,
        LongTime = 4,
        MonthAndYear = 5,
        YearOnly = 6,
        LongDateAndTime = 7,
        LongDateAndShortTime = 8
    }
    public enum NumericType
    {
        None = 0,
        Integer = 1,
        Double = 2,
        Decimal = 3,
        Single = 4,
        Int32 = 5
    }
    public enum Browser
    {
        UnIdentified = 0,
        Other = 1,
        Explorer = 2,
        Firefox = 3
    }
    public enum EmbeddedFileProcessingMethod
    {
        PageProcessing = 0,
        FileHandlerProcessing = 1
    }
    public enum UrlManagementType
    {
        Custom = 0,
        SinglePage = 1
    }
}
