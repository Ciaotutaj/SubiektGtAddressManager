using System.Windows;
using System.Windows.Input;
using Serilog;
using SubiektGtAddressManager.Services;
using SubiektGtAddressManager.ViewModels;

namespace SubiektGtAddressManager.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel = new();

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        if (!string.IsNullOrEmpty(_viewModel.InitialSqlPassword))
        {
            SqlPasswordBox.Password = _viewModel.InitialSqlPassword;
        }

        if (!string.IsNullOrEmpty(_viewModel.InitialOperatorPassword))
        {
            OperatorPasswordBox.Password = _viewModel.InitialOperatorPassword;
        }

        UpdateThemeButtonLabel();
        ThemeService.ThemeChanged += UpdateThemeButtonLabel;
        Closed += (_, _) => ThemeService.ThemeChanged -= UpdateThemeButtonLabel;
    }

    private void UpdateThemeButtonLabel()
        => ThemeToggleButton.Content = ThemeService.CurrentTheme == "Light" ? "Ciemny motyw" : "Jasny motyw";

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        ConnectButton.IsEnabled = false;
        Mouse.OverrideCursor = Cursors.Wait;

        var session = new SferaSession();

        Log.Information("Łączenie z {Server}/{Database} jako operator {Operator}",
            _viewModel.Server, _viewModel.Database, _viewModel.OperatorLogin);

        try
        {
            var success = _viewModel.Connect(session, SqlPasswordBox.Password, OperatorPasswordBox.Password, out var connectionString);

            if (!success)
            {
                Log.Warning("Połączenie nieudane: {Error}", _viewModel.ErrorMessage);
                session.Dispose();
                return;
            }

            Log.Information("Połączono pomyślnie");

            var mainWindow = new MainWindow(session, connectionString);
            mainWindow.Show();
            Close();
        }
        finally
        {
            Mouse.OverrideCursor = null;
            ConnectButton.IsEnabled = true;
        }
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e) => ThemeService.Toggle();

    private void ClearSavedLoginButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearSavedLogin();
        SqlPasswordBox.Password = string.Empty;
        OperatorPasswordBox.Password = string.Empty;
        Log.Information("Wyczyszczono zapisane dane logowania");
    }

    private async void LoadDatabasesButton_Click(object sender, RoutedEventArgs e)
    {
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            await _viewModel.LoadDatabasesAsync(SqlPasswordBox.Password);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private async void LoadOperatorsButton_Click(object sender, RoutedEventArgs e)
    {
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            await _viewModel.LoadOperatorsAsync(SqlPasswordBox.Password);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }
}
