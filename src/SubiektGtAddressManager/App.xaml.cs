using System.Configuration;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Serilog;
using SubiektGtAddressManager.Services;

namespace SubiektGtAddressManager;

public partial class App : Application
{
    private const string SingleInstanceMutexName = "SubiektGtAddressManager-SingleInstance";

    private Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            mutex.Dispose();

            MessageBox.Show(
                "Aplikacja jest już uruchomiona.",
                "SubiektGtAddressManager",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Shutdown();
            return;
        }

        _singleInstanceMutex = mutex;

        var logPath = Path.Combine(LogPaths.Directory, "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();

        Log.Information("Uruchamianie aplikacji");

        ThemeService.Apply(AppPreferences.Load().Theme);

        DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Zamykanie aplikacji");
        Log.CloseAndFlush();

        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();

        base.OnExit(e);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Nieobsłużony wyjątek");

        MessageBox.Show(
            $"Wystąpił nieoczekiwany błąd:\n\n{e.Exception.Message}",
            "Błąd aplikacji",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }
}

