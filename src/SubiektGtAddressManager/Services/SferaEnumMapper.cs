using InsERT;
using SubiektGtAddressManager.Models;

namespace SubiektGtAddressManager.Services;

public static class SferaEnumMapper
{
    public static AdresDostawyDodawajDoEFakturyEnum ToSfera(EInvoiceMode mode) => (AdresDostawyDodawajDoEFakturyEnum)(int)mode;

    public static AdresDostawyRolaEnum ToSfera(DeliveryRole role) => (AdresDostawyRolaEnum)(int)role;
}
