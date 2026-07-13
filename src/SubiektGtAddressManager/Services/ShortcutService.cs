using System.IO;
using System.Runtime.InteropServices;

namespace SubiektGtAddressManager.Services;

public static class ShortcutService
{
    public static string CreateDesktopShortcut()
    {
        var exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Nie udało się ustalić ścieżki pliku wykonywalnego.");

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var shortcutPath = Path.Combine(desktopPath, "SubiektGtAddressManager.lnk");

        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Brak dostępu do WScript.Shell (Windows Script Host).");

        dynamic shell = Activator.CreateInstance(shellType)!;

        try
        {
            dynamic shortcut = shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.IconLocation = exePath;
            shortcut.Description = "Zarządzanie adresami kontrahentów - Subiekt GT";
            shortcut.Save();

            Marshal.ReleaseComObject(shortcut);
        }
        finally
        {
            Marshal.ReleaseComObject(shell);
        }

        return shortcutPath;
    }
}
