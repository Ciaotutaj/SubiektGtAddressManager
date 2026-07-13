using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Serilog;
using SubiektGtAddressManager.Models;
using SubiektGtAddressManager.Services;

namespace SubiektGtAddressManager.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly SferaSession _session;
    private readonly ContractorGlnRepository _repository = new();
    private readonly string _connectionString;

    private string _searchText = string.Empty;
    private bool _showOnlyWithWarnings;
    private string _status = string.Empty;
    private bool _isBusy;
    private bool _isSaving;
    private double _saveProgress;

    private IReadOnlyList<ContractorGlnRow> _selectedRows = Array.Empty<ContractorGlnRow>();
    private EInvoiceMode _bulkEInvoiceMode;

    public MainViewModel(SferaSession session, string connectionString)
    {
        _session = session;
        _connectionString = connectionString;

        Rows = new ObservableCollection<ContractorGlnRow>();
        RowsView = CollectionViewSource.GetDefaultView(Rows);
        RowsView.Filter = FilterRow;

        if (RowsView is ICollectionViewLiveShaping liveShaping && liveShaping.CanChangeLiveFiltering)
        {
            liveShaping.LiveFilteringProperties.Add(nameof(ContractorGlnRow.WarningSummary));
            liveShaping.IsLiveFiltering = true;
        }

        RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsBusy);
        SaveCommand = new RelayCommand(async () => await SaveChangesAsync(), () => !IsBusy && Rows.Any(r => r.IsDirty));

        ApplyBulkEInvoiceModeCommand = new RelayCommand(() => ApplyBulkEdit(r => r.EInvoiceMode = BulkEInvoiceMode), () => !IsBusy && SelectedCount > 0);
    }

    public ObservableCollection<ContractorGlnRow> Rows { get; }

    public ICollectionView RowsView { get; }

    public RelayCommand RefreshCommand { get; }

    public RelayCommand SaveCommand { get; }

    public RelayCommand ApplyBulkEInvoiceModeCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RowsView.Refresh();
            }
        }
    }

    public bool ShowOnlyWithWarnings
    {
        get => _showOnlyWithWarnings;
        set
        {
            if (SetProperty(ref _showOnlyWithWarnings, value))
            {
                RowsView.Refresh();
            }
        }
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                ApplyBulkEInvoiceModeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

    public double SaveProgress
    {
        get => _saveProgress;
        private set => SetProperty(ref _saveProgress, value);
    }

    public int SelectedCount => _selectedRows.Count;

    public bool HasSelection => SelectedCount > 0;

    public EInvoiceMode BulkEInvoiceMode
    {
        get => _bulkEInvoiceMode;
        set => SetProperty(ref _bulkEInvoiceMode, value);
    }

    public void UpdateSelection(IReadOnlyList<ContractorGlnRow> selectedRows)
    {
        _selectedRows = selectedRows;
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));
        ApplyBulkEInvoiceModeCommand.RaiseCanExecuteChanged();
    }

    public async Task RefreshAsync()
    {
        if (Rows.Any(r => r.IsDirty))
        {
            Status = "Są niezapisane zmiany — zapisz je przed odświeżeniem albo odrzuć je ręcznie.";
            return;
        }

        IsBusy = true;
        Status = "Wczytywanie listy kontrahentów...";

        try
        {
            var list = await _repository.LoadListAsync(_connectionString);

            Rows.Clear();
            foreach (var row in list)
            {
                row.PropertyChanged += Row_PropertyChanged;
                Rows.Add(row);
            }

            Status = $"Wczytano {Rows.Count} kontrahentów.";
            Log.Information("Wczytano {Count} kontrahentów", Rows.Count);
        }
        catch (Exception ex)
        {
            Status = $"Błąd wczytywania: {ex.Message}";
            Log.Error(ex, "Nie udało się wczytać listy kontrahentów");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Row_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ContractorGlnRow.IsDirty))
        {
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task SaveChangesAsync()
    {
        var toSave = Rows.Where(r => r.IsDirty).ToList();
        if (toSave.Count == 0)
        {
            return;
        }

        IsBusy = true;
        IsSaving = true;
        SaveProgress = 0;

        var savedCount = 0;
        var errors = new List<string>();

        for (var i = 0; i < toSave.Count; i++)
        {
            var row = toSave[i];
            Status = $"Zapisywanie {i + 1} z {toSave.Count}: {row.Code}...";

            if (!GlnValidator.IsValidOrEmpty(row.Gln, out var error))
            {
                errors.Add($"{row.Code}: {error}");
            }
            else if (!GlnValidator.IsValidOrEmpty(row.DeliveryGln, out var deliveryError))
            {
                errors.Add($"{row.Code} (GLN dostawy): {deliveryError}");
            }
            else if (!GlnValidator.IsValidOrEmpty(row.CorrespondenceGln, out var correspondenceError))
            {
                errors.Add($"{row.Code} (GLN korespondencyjny): {correspondenceError}");
            }
            else
            {
                try
                {
                    _repository.SaveContractor(_session, row);
                    row.MarkSaved();
                    savedCount++;
                    Log.Information("Zapisano dane kontrahenta {Code}", row.Code);
                }
                catch (Exception ex)
                {
                    errors.Add($"{row.Code}: {ex.Message}");
                    Log.Error(ex, "Nie udało się zapisać danych kontrahenta {Code}", row.Code);
                }
            }

            SaveProgress = (i + 1) * 100.0 / toSave.Count;

            await Task.Yield();
        }

        IsBusy = false;
        IsSaving = false;

        Status = errors.Count == 0
            ? $"Zapisano {savedCount} z {toSave.Count} zmian."
            : $"Zapisano {savedCount} z {toSave.Count} zmian. Błędy: {string.Join(" | ", errors)}";
    }

    private void ApplyBulkEdit(Action<ContractorGlnRow> apply)
    {
        if (_selectedRows.Count == 0)
        {
            return;
        }

        foreach (var row in _selectedRows)
        {
            apply(row);
        }

        Status = $"Zastosowano zmianę zbiorczą dla {_selectedRows.Count} kontrahentów.";
    }

    private bool FilterRow(object obj)
    {
        if (obj is not ContractorGlnRow row)
        {
            return false;
        }

        if (ShowOnlyWithWarnings && row.WarningSummary.Length == 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var phrase = SearchText.Trim();
        return ContainsText(row.Code, phrase) || ContainsText(row.Name, phrase) || ContainsText(row.Nip, phrase);
    }

    private static bool ContainsText(string? text, string phrase)
        => !string.IsNullOrEmpty(text) && text.Contains(phrase, StringComparison.OrdinalIgnoreCase);
}
