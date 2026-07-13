using System.Windows;

namespace SubiektGtAddressManager.Services;

public static class ThemeService
{
    private static string _currentTheme = "Light";
    private static ResourceDictionary? _activeDictionary;

    public static string CurrentTheme => _currentTheme;

    public static event Action? ThemeChanged;

    public static void Apply(string theme)
    {
        _currentTheme = theme;

        var uri = new Uri($"/Themes/{theme}.xaml", UriKind.Relative);
        var dictionary = new ResourceDictionary { Source = uri };

        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        var toRemove = _activeDictionary ?? mergedDictionaries.FirstOrDefault(d => d.Contains("WindowBackgroundBrush"));
        if (toRemove is not null)
        {
            mergedDictionaries.Remove(toRemove);
        }

        mergedDictionaries.Insert(0, dictionary);
        _activeDictionary = dictionary;

        ThemeChanged?.Invoke();
    }

    public static void Toggle()
    {
        var next = _currentTheme == "Light" ? "Dark" : "Light";
        Apply(next);

        var preferences = AppPreferences.Load();
        preferences.Theme = next;
        AppPreferences.Save(preferences);
    }
}
