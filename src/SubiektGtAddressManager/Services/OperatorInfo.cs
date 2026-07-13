namespace SubiektGtAddressManager.Services;

public sealed record OperatorInfo(string Code, string FirstName, string LastName)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}
