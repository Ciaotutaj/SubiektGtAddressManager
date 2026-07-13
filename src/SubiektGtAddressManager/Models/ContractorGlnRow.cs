using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SubiektGtAddressManager.Models;

public sealed class ContractorGlnRow : INotifyPropertyChanged
{
    private string? _gln;

    private string? _correspondenceGln;
    private bool _useCorrespondenceAddress;

    private string? _deliveryGln;
    private EInvoiceMode _eInvoiceMode;
    private DeliveryRole _deliveryRole = DeliveryRole.Recipient;

    private AddressFields _originalHeadOffice = new();
    private AddressFields _originalCorrespondence = new();
    private AddressFields _originalDelivery = new();

    public ContractorGlnRow()
    {
        HeadOffice.PropertyChanged += (_, _) => OnAddressFieldChanged();
        Correspondence.PropertyChanged += (_, _) => OnAddressFieldChanged();
        Delivery.PropertyChanged += (_, _) => OnAddressFieldChanged();
    }

    public required int ContractorId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Nip { get; init; }

    public AddressFields HeadOffice { get; } = new();
    public AddressFields Correspondence { get; } = new();
    public AddressFields Delivery { get; } = new();

    public string? OriginalGln { get; internal set; }
    public string? OriginalCorrespondenceGln { get; internal set; }
    public bool OriginalUseCorrespondenceAddress { get; internal set; }
    public string? OriginalDeliveryGln { get; internal set; }
    public EInvoiceMode OriginalEInvoiceMode { get; internal set; }
    public DeliveryRole OriginalDeliveryRole { get; internal set; } = DeliveryRole.Recipient;

    public string? Gln { get => _gln; set => SetField(ref _gln, value); }

    public string? CorrespondenceGln { get => _correspondenceGln; set => SetField(ref _correspondenceGln, value); }

    public bool UseCorrespondenceAddress
    {
        get => _useCorrespondenceAddress;
        set => SetField(ref _useCorrespondenceAddress, value);
    }

    public string? DeliveryGln { get => _deliveryGln; set => SetField(ref _deliveryGln, value); }
    public EInvoiceMode EInvoiceMode { get => _eInvoiceMode; set => SetField(ref _eInvoiceMode, value); }
    public DeliveryRole DeliveryRole { get => _deliveryRole; set => SetField(ref _deliveryRole, value); }

    public bool IsHeadOfficeAddressComplete => HeadOffice.IsComplete;

    public bool IsCorrespondenceAddressComplete => Correspondence.IsComplete;

    public bool IsDeliveryAddressComplete => Delivery.IsComplete;

    public bool HeadOfficeGlnWarning => HasText(Gln) && !IsHeadOfficeAddressComplete;

    public bool CorrespondenceGlnWarning => UseCorrespondenceAddress && HasText(CorrespondenceGln) && !IsCorrespondenceAddressComplete;

    public bool DeliveryGlnWarning => EInvoiceMode != EInvoiceMode.None && HasText(DeliveryGln) && !IsDeliveryAddressComplete;

    public string WarningSummary
    {
        get
        {
            var warnings = new List<string>();
            if (HeadOfficeGlnWarning)
            {
                warnings.Add("siedziba");
            }

            if (CorrespondenceGlnWarning)
            {
                warnings.Add("koresp.");
            }

            if (DeliveryGlnWarning)
            {
                warnings.Add("dostawa");
            }

            return warnings.Count == 0 ? string.Empty : "⚠ " + string.Join(", ", warnings);
        }
    }

    public bool IsDirty =>
        !string.Equals(Normalize(Gln), Normalize(OriginalGln), StringComparison.Ordinal) ||
        HeadOffice.DiffersFrom(_originalHeadOffice) ||
        !string.Equals(Normalize(CorrespondenceGln), Normalize(OriginalCorrespondenceGln), StringComparison.Ordinal) ||
        UseCorrespondenceAddress != OriginalUseCorrespondenceAddress ||
        Correspondence.DiffersFrom(_originalCorrespondence) ||
        !string.Equals(Normalize(DeliveryGln), Normalize(OriginalDeliveryGln), StringComparison.Ordinal) ||
        EInvoiceMode != OriginalEInvoiceMode ||
        DeliveryRole != OriginalDeliveryRole ||
        Delivery.DiffersFrom(_originalDelivery);

    public void MarkSaved()
    {
        OriginalGln = Gln;
        _originalHeadOffice = HeadOffice.Snapshot();

        OriginalCorrespondenceGln = CorrespondenceGln;
        OriginalUseCorrespondenceAddress = UseCorrespondenceAddress;
        _originalCorrespondence = Correspondence.Snapshot();

        OriginalDeliveryGln = DeliveryGln;
        OriginalEInvoiceMode = EInvoiceMode;
        OriginalDeliveryRole = DeliveryRole;
        _originalDelivery = Delivery.Snapshot();

        OnPropertyChanged(nameof(IsDirty));
    }

    private static bool HasText(string? value) => !string.IsNullOrWhiteSpace(value);

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    private void OnAddressFieldChanged()
    {
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(IsHeadOfficeAddressComplete));
        OnPropertyChanged(nameof(IsCorrespondenceAddressComplete));
        OnPropertyChanged(nameof(IsDeliveryAddressComplete));
        OnPropertyChanged(nameof(HeadOfficeGlnWarning));
        OnPropertyChanged(nameof(CorrespondenceGlnWarning));
        OnPropertyChanged(nameof(DeliveryGlnWarning));
        OnPropertyChanged(nameof(WarningSummary));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(IsHeadOfficeAddressComplete));
        OnPropertyChanged(nameof(IsCorrespondenceAddressComplete));
        OnPropertyChanged(nameof(IsDeliveryAddressComplete));
        OnPropertyChanged(nameof(HeadOfficeGlnWarning));
        OnPropertyChanged(nameof(CorrespondenceGlnWarning));
        OnPropertyChanged(nameof(DeliveryGlnWarning));
        OnPropertyChanged(nameof(WarningSummary));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
