namespace SubiektGtAddressManager.Services;

public sealed class ConnectionParameters
{
    public required string Server { get; init; }
    public required string Database { get; init; }
    public required bool UseWindowsAuthentication { get; init; }
    public string? SqlUsername { get; init; }
    public string? SqlPassword { get; init; }
    public required string OperatorLogin { get; init; }
    public string? OperatorPassword { get; init; }
}
