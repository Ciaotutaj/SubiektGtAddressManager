using System.Collections.ObjectModel;
using SubiektGtAddressManager.Services;

namespace SubiektGtAddressManager.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private string _server = string.Empty;
    private string _database = string.Empty;
    private bool _useWindowsAuthentication = true;
    private string _sqlUsername = string.Empty;
    private string _operatorLogin = string.Empty;
    private string _operatorDisplayText = string.Empty;
    private bool _rememberLogin;
    private bool _isConnecting;
    private bool _isLoadingDatabases;
    private bool _isLoadingOperators;
    private string? _errorMessage;

    public LoginViewModel()
    {
        var saved = SavedLoginSettings.Load();
        if (saved is not null)
        {
            _server = saved.Server;
            _database = saved.Database;
            _useWindowsAuthentication = saved.UseWindowsAuthentication;
            _sqlUsername = saved.SqlUsername;
            _operatorLogin = saved.OperatorLogin;
            _operatorDisplayText = saved.OperatorLogin;
            _rememberLogin = true;
            InitialSqlPassword = saved.SqlPassword;
            InitialOperatorPassword = saved.OperatorPassword;
        }
    }

    public string? InitialSqlPassword { get; }

    public string? InitialOperatorPassword { get; }

    public ObservableCollection<string> Databases { get; } = new();

    public ObservableCollection<OperatorInfo> Operators { get; } = new();

    public string Server
    {
        get => _server;
        set => SetProperty(ref _server, value);
    }

    public string Database
    {
        get => _database;
        set => SetProperty(ref _database, value);
    }

    public bool UseWindowsAuthentication
    {
        get => _useWindowsAuthentication;
        set => SetProperty(ref _useWindowsAuthentication, value);
    }

    public string SqlUsername
    {
        get => _sqlUsername;
        set => SetProperty(ref _sqlUsername, value);
    }

    public string OperatorLogin
    {
        get => _operatorLogin;
        set => SetProperty(ref _operatorLogin, value);
    }

    public string OperatorDisplayText
    {
        get => _operatorDisplayText;
        set
        {
            if (SetProperty(ref _operatorDisplayText, value))
            {
                var trimmed = value.Trim();
                var match = Operators.FirstOrDefault(o =>
                    string.Equals(o.FullName, trimmed, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(o.Code, trimmed, StringComparison.OrdinalIgnoreCase));

                OperatorLogin = match?.Code ?? trimmed;
            }
        }
    }

    public bool RememberLogin
    {
        get => _rememberLogin;
        set => SetProperty(ref _rememberLogin, value);
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        private set => SetProperty(ref _isConnecting, value);
    }

    public bool IsLoadingDatabases
    {
        get => _isLoadingDatabases;
        private set => SetProperty(ref _isLoadingDatabases, value);
    }

    public bool IsLoadingOperators
    {
        get => _isLoadingOperators;
        private set => SetProperty(ref _isLoadingOperators, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public async Task LoadDatabasesAsync(string sqlPassword)
    {
        ErrorMessage = null;
        IsLoadingDatabases = true;

        try
        {
            var list = await SqlServerBrowser.ListDatabasesAsync(Server.Trim(), UseWindowsAuthentication, SqlUsername.Trim(), sqlPassword);
            Databases.Clear();
            foreach (var name in list)
            {
                Databases.Add(name);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się pobrać listy baz danych: {ex.Message}";
        }
        finally
        {
            IsLoadingDatabases = false;
        }
    }

    public async Task LoadOperatorsAsync(string sqlPassword)
    {
        if (string.IsNullOrWhiteSpace(Database))
        {
            ErrorMessage = "Najpierw podaj nazwę bazy danych.";
            return;
        }

        ErrorMessage = null;
        IsLoadingOperators = true;

        try
        {
            var list = await SqlServerBrowser.ListOperatorsAsync(Server.Trim(), UseWindowsAuthentication, SqlUsername.Trim(), sqlPassword, Database.Trim());
            Operators.Clear();
            foreach (var op in list)
            {
                Operators.Add(op);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nie udało się pobrać listy operatorów: {ex.Message}";
        }
        finally
        {
            IsLoadingOperators = false;
        }
    }

    public void ClearSavedLogin()
    {
        SavedLoginSettings.Delete();

        Server = string.Empty;
        Database = string.Empty;
        UseWindowsAuthentication = true;
        SqlUsername = string.Empty;
        OperatorDisplayText = string.Empty;
        RememberLogin = false;
        ErrorMessage = null;
    }

    public bool Connect(SferaSession session, string sqlPassword, string operatorPassword, out string connectionString)
    {
        ErrorMessage = null;
        IsConnecting = true;
        connectionString = string.Empty;

        try
        {
            var parameters = new ConnectionParameters
            {
                Server = Server.Trim(),
                Database = Database.Trim(),
                UseWindowsAuthentication = UseWindowsAuthentication,
                SqlUsername = SqlUsername.Trim(),
                SqlPassword = sqlPassword,
                OperatorLogin = OperatorLogin.Trim(),
                OperatorPassword = operatorPassword,
            };

            session.Connect(parameters);
            connectionString = ContractorGlnRepository.BuildConnectionString(parameters);

            if (RememberLogin)
            {
                SavedLoginSettings.Save(new SavedLoginSettings
                {
                    Server = parameters.Server,
                    Database = parameters.Database,
                    UseWindowsAuthentication = parameters.UseWindowsAuthentication,
                    SqlUsername = parameters.SqlUsername ?? string.Empty,
                    SqlPassword = sqlPassword,
                    OperatorLogin = parameters.OperatorLogin,
                    OperatorPassword = operatorPassword,
                });
            }
            else
            {
                SavedLoginSettings.Delete();
            }

            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            IsConnecting = false;
        }
    }
}
