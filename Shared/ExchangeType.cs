namespace Localist.Shared
{
    // todo?: split into two enums e.g. TransactionDirection (In, Out) and TransactionType (BuySell, BorrowLend, etc)
    public enum ExchangeType
    {
        // even: money TO poster
        Sell = 0,
        Loan = 2,
        Service = 4,
        Let = 6,

        // odd: money FROM poster
        Buy = 1,
        Borrow = 3,
        Hire = 5,
        Rent = 7,
    }
}
