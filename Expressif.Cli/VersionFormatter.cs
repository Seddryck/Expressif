using System.Reflection;

namespace Expressif.Cli;

internal static class VersionFormatter
{
    public static string GetVersion(Assembly assembly)
    {
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
            return informational.Split('+')[0];

        var version = assembly.GetName().Version;
        if (version is null)
            return "unknown";

        if (version.Build < 0)
            return $"{version.Major}.{version.Minor}";

        if (version.Revision < 0)
            return $"{version.Major}.{version.Minor}.{version.Build}";

        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
