using System.IO;

namespace SubiektGtAddressManager.Services;

public static class LogPaths
{
    public static string Directory => Path.Combine(AppContext.BaseDirectory, "logs");
}
