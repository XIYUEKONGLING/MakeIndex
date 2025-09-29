using Newtonsoft.Json;

namespace MakeIndex.Models;

[Serializable, JsonObject]
public sealed class Version : IComparable<Version>, IEquatable<Version>, IComparable
{
    [JsonProperty]
    public long Major { get; set; }
    
    [JsonProperty]
    public long Minor { get; set; }
    
    [JsonProperty]
    public long Patch { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public long? Build { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? Tag { get; set; }
    
    public Version()
    {
        Major = 0;
        Minor = 0;
        Patch = 0;
        Build = null;
        Tag = null;
    }
    
    public Version(long major, long minor, long patch, long? build = null, string? tag = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Build = build;
        Tag = tag;
    }

    /// <summary>
    /// Compares only the Major, Minor, and Patch components
    /// </summary>
    public int CompareCoreVersionTo(Version? other) => 
        other is null ? 1 : 
        Major.CompareTo(other.Major) is var cmp && cmp != 0 ? cmp :
        Minor.CompareTo(other.Minor) is var cmp2 && cmp2 != 0 ? cmp2 :
        Patch.CompareTo(other.Patch);

    /// <summary>
    /// Determines if core version (Major, Minor, Patch) is equal
    /// </summary>
    public bool CoreVersionEquals(Version? other) => 
        other is not null && 
        Major == other.Major && 
        Minor == other.Minor && 
        Patch == other.Patch;

    #region IComparable Implementation
    
    public int CompareTo(Version? other) => other is null ? 1 :
        Major.CompareTo(other.Major) is var cmp && cmp != 0 ? cmp :
        Minor.CompareTo(other.Minor) is var cmp2 && cmp2 != 0 ? cmp2 :
        Patch.CompareTo(other.Patch) is var cmp3 && cmp3 != 0 ? cmp3 :
        (Build ?? 0).CompareTo(other.Build ?? 0);

    public int CompareTo(object? obj) => 
        obj is Version version ? CompareTo(version) : 
        throw new ArgumentException("Object must be of type Version", nameof(obj));
    
    #endregion

    #region IEquatable Implementation
    
    public bool Equals(Version? other) => 
        other is not null && 
        Major == other.Major &&
        Minor == other.Minor &&
        Patch == other.Patch &&
        (Build ?? 0) == (other.Build ?? 0) &&
        string.Equals(Tag, other.Tag, StringComparison.OrdinalIgnoreCase);
    
    #endregion

    #region Object Overrides
    
    public override bool Equals(object? obj) => obj is Version other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        Major, Minor, Patch, Build ?? 0, Tag?.ToLowerInvariant());

    public override string ToString() => 
        Build.HasValue
            ? $"{Major}.{Minor}.{Patch}.{Build}{(string.IsNullOrEmpty(Tag) ? "" : $"-{Tag}")}"
            : $"{Major}.{Minor}.{Patch}{(string.IsNullOrEmpty(Tag) ? "" : $"-{Tag}")}";
    
    #endregion

    #region Operators
    
    public static bool operator ==(Version? left, Version? right) => 
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Version? left, Version? right) => !(left == right);
    public static bool operator <(Version? left, Version? right) => 
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator >(Version? left, Version? right) => 
        left is not null && left.CompareTo(right) > 0;

    public static bool operator <=(Version? left, Version? right) => 
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >=(Version? left, Version? right) => 
        left is null ? right is null : left.CompareTo(right) >= 0;
    
    #endregion

    #region Static Methods
    
    public static Version Parse(string version) => 
        TryParse(version, out var result) 
            ? result 
            : throw new ArgumentException($"Invalid version format: {version}", nameof(version));

    public static bool TryParse(string? version, out Version result)
    {
        result = null!;
    
        if (string.IsNullOrWhiteSpace(version))
            return false;

        var parts = version.Split('-', 2);
        var versionPart = parts[0];
        var tag = parts.Length > 1 ? parts[1] : null;

        var segments = versionPart.Split('.');
        if (segments.Length is < 3 or > 4)
            return false;

        if (!TryParseSegment(segments[0], out var major) ||
            !TryParseSegment(segments[1], out var minor) ||
            !TryParseSegment(segments[2], out var patch))
            return false;

        long? build = null;
        if (segments.Length == 4)
        {
            if (!TryParseSegment(segments[3], out var buildValue))
                return false;
            build = buildValue;
        }

        result = new Version(major, minor, patch, build, tag);
        return true;

        static bool TryParseSegment(string segment, out long value)
        {
            value = 0;
            return long.TryParse(segment, out value) && value >= 0;
        }
    }
    
    #endregion

    #region Utility Methods
    
    public bool IsCompatibleWith(Version minVersion, Version? maxVersion = null) => 
        this >= minVersion && (maxVersion is null || this <= maxVersion);

    [JsonIgnore]
    public bool IsPreRelease => Build.HasValue;
    
    [JsonIgnore]
    public bool HasTag => !string.IsNullOrEmpty(Tag);
    
    public Version GetReleaseVersion() => new(Major, Minor, Patch);
    public Version GetVersionWithoutTag() => new(Major, Minor, Patch, Build);
    public Version WithTag(string? tag) => new(Major, Minor, Patch, Build, tag);
    public Version WithoutTag() => new(Major, Minor, Patch, Build);
    public Version IncrementMajor() => new(Major + 1, 0, 0, null, Tag);
    public Version IncrementMinor() => new(Major, Minor + 1, 0, null, Tag);
    public Version IncrementPatch() => new(Major, Minor, Patch + 1, null, Tag);
    public Version IncrementBuild() => new(Major, Minor, Patch, (Build ?? 0) + 1, Tag);
    
    #endregion
}
