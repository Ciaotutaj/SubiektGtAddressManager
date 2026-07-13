namespace SubiektGtAddressManager.Models;

public enum EInvoiceMode
{
    None = 0,
    AsThirdParty = 1,
    TransactionTerms = 2,
}

public sealed class EInvoiceModeOption
{
    public EInvoiceMode Value { get; init; }
    public string Label { get; init; } = string.Empty;
}

public static class EInvoiceModeOptions
{
    public static IReadOnlyList<EInvoiceModeOption> All { get; } = new List<EInvoiceModeOption>
    {
        new() { Value = EInvoiceMode.None, Label = "Nie dodawaj" },
        new() { Value = EInvoiceMode.AsThirdParty, Label = "Jako Podmiot 3" },
        new() { Value = EInvoiceMode.TransactionTerms, Label = "W warunkach transakcji" },
    };
}
