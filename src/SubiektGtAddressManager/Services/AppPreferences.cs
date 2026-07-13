using System.IO;
using System.Text.Json;

namespace SubiektGtAddressManager.Services;

public sealed class AppPreferences
{
    public string Theme { get; set; } = "Light";

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SubiektGtAddressManager",
        "preferences.json");

    public static void Save(AppPreferences preferences)
    {
        var path = FilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(preferences));
    }

    public static AppPreferences Load()
    {
        try
        {
            var path = FilePath;
            if (!File.Exists(path))
            {
                return new AppPreferences();
            }

            return JsonSerializer.Deserialize<AppPreferences>(File.ReadAllText(path)) ?? new AppPreferences();
        }
        catch (IOException)
        {
            return new AppPreferences();
        }
        catch (JsonException)
        {
            return new AppPreferences();
        }
    }
}
