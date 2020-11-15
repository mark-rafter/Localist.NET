namespace Localist.Shared
{
    public interface IAccountResult
    {
        bool Successful { get; }
        string? Error { get; }
        string? Token { get; }
    }

    public record AccountResult(bool Successful, string? Error = null, string? Token = null) : IAccountResult;

    public record FailedAccountResult(string Error) : AccountResult(false, Error);
    public record SuccessfulAccountResult(string Token) : AccountResult(true, null, Token);
}
