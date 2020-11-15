namespace Localist.Shared
{
    public record ExchangeDetails(
        decimal Price,
        ExchangeType Type,
        string? Unit, // e.g. each, per kg, per hour
        decimal? Quantity = null,
        string? Currency = null, // e.g. $/£/BTC
        bool IsExpired = false // item bought/sold, service no longer available/needed, etc.
        )
    {
        public string GetPrice()
        {
            return Price switch
            {
                <= 0 => "FREE",
                // < 1 => $"{Math.Round(Price * 100)}p {Unit}", // todo: currency subunits
                _ => $"{Currency ?? "£"}{Price}" + (Unit is not null ? $" {Unit}" : "")
            };
        }
    }
}
