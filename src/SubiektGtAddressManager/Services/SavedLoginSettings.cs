using System.IO;
using System.Text.Json;

namespace SubiektGtAddressManager.Services;

public sealed class SavedLoginSettings
{
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public bool UseWindowsAuthentication { get; set; } = true;
    public string SqlUsername { get; set; } = string.Empty;
    public string SqlPassword { get; set; } = string.Empty;
    public string OperatorLogin { get; set; } = string.Empty;
    public string OperatorPassword { get; set; } = string.Empty;

    private static string FilePath => Path.Combine(ConfigPaths.Directory, "logowanie.json");

    public static void Save(SavedLoginSettings settings)
    {
        var path = FilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(settings));
    }

    public static SavedLoginSettings? Load()
    {
        try
        {
            var path = FilePath;
            return File.Exists(path)
                ? JsonSerializer.Deserialize<SavedLoginSettings>(File.ReadAllText(path))
                : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static void Delete()
    {
        var path = FilePath;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
