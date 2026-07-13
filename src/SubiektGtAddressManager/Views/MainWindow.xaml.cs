using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;
using SubiektGtAddressManager.Models;
using SubiektGtAddressManager.Services;
using SubiektGtAddressManager.ViewModels;

namespace SubiektGtAddressManager.Views;

public partial class MainWindow : Window
{
    private readonly SferaSession _session;
    private readonly MainViewModel _viewModel;

    public MainWindow(SferaSession session, string connectionString)
    {
        InitializeComponent();

        _session = session;
        _viewModel = new MainViewModel(session, connectionString);
        DataContext = _viewModel;

        Loaded += async (_, _) => await _viewModel.RefreshAsync();

        UpdateThemeButtonLabel();
        ThemeService.ThemeChanged += UpdateThemeButtonLabel;
        Closed += (_, _) => ThemeService.ThemeChanged -= UpdateThemeButtonLabel;
    }

    private void UpdateThemeButtonLabel()
        => ThemeToggleButton.Content = ThemeService.CurrentTheme == "Light" ? "Ciemny motyw" : "Jasny motyw";

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_viewModel.Rows.Any(r => r.IsDirty))
        {
            var result = MessageBox.Show(
                "Masz niezapisane zmiany. Czy na pewno chcesz zamknąć program bez zapisu?",
                "Niezapisane zmiany",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        Log.Information("Zamykanie głównego okna");
        _session.Dispose();
    }

    private void ContractorsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => _viewModel.UpdateSelection(ContractorsGrid.SelectedItems.Cast<ContractorGlnRow>().ToList());

    private void FocusSearch_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        SearchTextBox.Focus();
        SearchTextBox.SelectAll();
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e) => ThemeService.Toggle();

    private void CreateShortcutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = ShortcutService.CreateDesktopShortcut();
            MessageBox.Show($"Utworzono skrót na pulpicie:\n{path}", "Skrót utworzony", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Nie udało się utworzyć skrótu na pulpicie");
            MessageBox.Show($"Nie udało się utworzyć skrótu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenLogsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(LogPaths.Directory);
            Process.Start(new ProcessStartInfo { FileName = LogPaths.Directory, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Nie udało się otworzyć folderu logów");
            MessageBox.Show($"Nie udało się otworzyć folderu logów:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
