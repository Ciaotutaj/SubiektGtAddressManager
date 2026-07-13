namespace SubiektGtAddressManager.Models;

public enum DeliveryRole
{
    Other = 0,
    Factor = 1,
    Recipient = 2,
    OriginalEntity = 3,
    AdditionalBuyer = 4,
    InvoiceIssuer = 5,
    PaymentMaker = 6,
    LocalGovernmentIssuer = 7,
    LocalGovernmentRecipient = 8,
    VatGroupIssuer = 9,
    VatGroupRecipient = 10,
    Employee = 11,
    None = 12,
}

public sealed class DeliveryRoleOption
{
    public DeliveryRole Value { get; init; }
    public string Label { get; init; } = string.Empty;
}

public static class DeliveryRoleOptions
{
    public static IReadOnlyList<DeliveryRoleOption> All { get; } = new List<DeliveryRoleOption>
    {
        new() { Value = DeliveryRole.Other, Label = "Inna" },
        new() { Value = DeliveryRole.Factor, Label = "Faktor" },
        new() { Value = DeliveryRole.Recipient, Label = "Odbiorca" },
        new() { Value = DeliveryRole.OriginalEntity, Label = "Podmiot pierwotny" },
        new() { Value = DeliveryRole.AdditionalBuyer, Label = "Dodatkowy nabywca" },
        new() { Value = DeliveryRole.InvoiceIssuer, Label = "Wystawca faktury" },
        new() { Value = DeliveryRole.PaymentMaker, Label = "Dokonujący płatności" },
        new() { Value = DeliveryRole.LocalGovernmentIssuer, Label = "JST - Wystawca" },
        new() { Value = DeliveryRole.LocalGovernmentRecipient, Label = "JST - Odbiorca" },
        new() { Value = DeliveryRole.VatGroupIssuer, Label = "CGV - Wystawca" },
        new() { Value = DeliveryRole.VatGroupRecipient, Label = "CGV - Odbiorca" },
        new() { Value = DeliveryRole.Employee, Label = "Pracownik" },
        new() { Value = DeliveryRole.None, Label = "Brak" },
    };
}
