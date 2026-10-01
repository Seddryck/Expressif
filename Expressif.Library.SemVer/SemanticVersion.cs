using System.Globalization;
using System.Text.RegularExpressions;

namespace Expressif.Library.SemVer;

/// <summary>Represents a Semantic Versioning 2.0.0 value.</summary>
public sealed class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
{
    private static readonly Regex Pattern = new(
        "^(?<major>0|[1-9][0-9]*)\\.(?<minor>0|[1-9][0-9]*)\\.(?<patch>0|[1-9][0-9]*)"
        + "(?:-(?<pre>[0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?"
        + "(?:\\+(?<build>[0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?$",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    public SemanticVersion(int major, int minor, int patch, string? preRelease = null, string? buildMetadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);
        ArgumentOutOfRangeException.ThrowIfNegative(patch);
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        BuildMetadata = buildMetadata;
    }

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? PreRelease { get; }
    public string? BuildMetadata { get; }

    public static bool TryParse(string? text, out SemanticVersion? version)
    {
        version = null;
        if (text is null)
            return false;
        var match = Pattern.Match(text);
        if (!match.Success
            || !int.TryParse(match.Groups["major"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var major)
            || !int.TryParse(match.Groups["minor"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var minor)
            || !int.TryParse(match.Groups["patch"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var patch))
        {
            return false;
        }
        var preRelease = match.Groups["pre"].Success ? match.Groups["pre"].Value : null;
        if (preRelease?.Split('.').Any(identifier => identifier.Length > 1
            && identifier[0] == '0'
            && identifier.All(char.IsAsciiDigit)) == true)
        {
            return false;
        }
        version = new SemanticVersion(
            major,
            minor,
            patch,
            preRelease,
            match.Groups["build"].Success ? match.Groups["build"].Value : null);
        return true;
    }

    public int CompareTo(SemanticVersion? other)
    {
        if (other is null)
            return 1;
        var core = Major.CompareTo(other.Major);
        if (core == 0)
            core = Minor.CompareTo(other.Minor);
        if (core == 0)
            core = Patch.CompareTo(other.Patch);
        return core != 0 ? core : ComparePreRelease(PreRelease, other.PreRelease);
    }

    public bool Equals(SemanticVersion? other)
        => other is not null
            && Major == other.Major
            && Minor == other.Minor
            && Patch == other.Patch
            && string.Equals(PreRelease, other.PreRelease, StringComparison.Ordinal)
            && string.Equals(BuildMetadata, other.BuildMetadata, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as SemanticVersion);

    public override int GetHashCode()
        => HashCode.Combine(Major, Minor, Patch, PreRelease, BuildMetadata);

    public override string ToString()
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{Major}.{Minor}.{Patch}{(PreRelease is null ? string.Empty : $"-{PreRelease}")}{(BuildMetadata is null ? string.Empty : $"+{BuildMetadata}")}");

    private static int ComparePreRelease(string? left, string? right)
    {
        if (left is null)
            return right is null ? 0 : 1;
        if (right is null)
            return -1;
        var leftIdentifiers = left.Split('.');
        var rightIdentifiers = right.Split('.');
        for (var index = 0; index < Math.Min(leftIdentifiers.Length, rightIdentifiers.Length); index++)
        {
            var comparison = CompareIdentifier(leftIdentifiers[index], rightIdentifiers[index]);
            if (comparison != 0)
                return comparison;
        }
        return leftIdentifiers.Length.CompareTo(rightIdentifiers.Length);
    }

    private static int CompareIdentifier(string left, string right)
    {
        var leftNumeric = left.All(char.IsAsciiDigit);
        var rightNumeric = right.All(char.IsAsciiDigit);
        if (leftNumeric && rightNumeric)
            return CompareNumericIdentifier(left, right);
        if (leftNumeric != rightNumeric)
            return leftNumeric ? -1 : 1;
        return string.Compare(left, right, StringComparison.Ordinal);
    }

    private static int CompareNumericIdentifier(string left, string right)
    {
        var length = left.Length.CompareTo(right.Length);
        return length != 0 ? length : string.Compare(left, right, StringComparison.Ordinal);
    }
}
