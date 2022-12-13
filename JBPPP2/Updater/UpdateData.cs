using Newtonsoft.Json;

namespace JBPPP2;

internal class UpdateData
{
    [JsonProperty("version")]
    internal string VersionStr { get; set; }
    
    [JsonProperty("downloadUrl")]
    internal string DownloadUrl { get; set; }
    
    [JsonIgnore]
    internal Version Version => Version.Parse(VersionStr);
}