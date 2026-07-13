using System.IO;

namespace SubiektGtAddressManager.Services;

public static class ConfigPaths
{
    public static string Directory => Path.Combine(AppContext.BaseDirectory, "config");
}
