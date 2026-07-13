using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SubiektGtAddressManager.Models;

public sealed class AddressFields : INotifyPropertyChanged
{
    private string? _street;
    private string? _houseNumber;
    private string? _postalCode;
    private string? _city;

    public string? Street { get => _street; set => SetField(ref _street, value); }
    public string? HouseNumber { get => _houseNumber; set => SetField(ref _houseNumber, value); }
    public string? PostalCode { get => _postalCode; set => SetField(ref _postalCode, value); }
    public string? City { get => _city; set => SetField(ref _city, value); }

    public bool IsComplete => HasText(Street) && HasText(HouseNumber) && HasText(PostalCode) && HasText(City);

    public AddressFields Snapshot() => new()
    {
        Street = Street,
        HouseNumber = HouseNumber,
        PostalCode = PostalCode,
        City = City,
    };

    public bool DiffersFrom(AddressFields other) =>
        !string.Equals(Normalize(Street), Normalize(other.Street), StringComparison.Ordinal) ||
        !string.Equals(Normalize(HouseNumber), Normalize(other.HouseNumber), StringComparison.Ordinal) ||
        !string.Equals(Normalize(PostalCode), Normalize(other.PostalCode), StringComparison.Ordinal) ||
        !string.Equals(Normalize(City), Normalize(other.City), StringComparison.Ordinal);

    private static bool HasText(string? value) => !string.IsNullOrWhiteSpace(value);

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
