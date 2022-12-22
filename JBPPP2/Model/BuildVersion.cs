using System.Text.RegularExpressions;

namespace JBPPP2.Model;

internal class BuildVersion
{
    private const string Pattern = "(?<major>[-]?[0-9]+)[-\\.:](?<minor>[-]?[0-9]+)[-\\.:](?<short>[A-Z][A-Z])";
    
    public int Major { get; }
    
    public int? Patch { get; }
    
    public string? Localization { get; }
    
    public bool IsPatchInstalled => Localization != null;

    internal BuildVersion(string regex)
    {
        var match = Regex.Match(regex, Pattern);
        if (match.Success)
        {
            Major = int.Parse(match.Groups["major"].Value);
            Patch = int.Parse(match.Groups["minor"].Value);
            Localization = match.Groups["short"].Value;
        }
    }
    
    internal bool IsNewerThan(BuildVersion other)
    {
        if (Major > other.Major)
            return true;
        if (Major < other.Major)
            return false;
        if (Patch != null && other.Patch != null)
            return Patch > other.Patch;
        return false;
    }
}